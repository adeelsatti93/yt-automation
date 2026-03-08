using System.Diagnostics;
using System.Text;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Exceptions;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.Video;

public class FfmpegVideoService : IVideoAssemblyService
{
    private readonly IAssetStorageService _storage;
    private readonly ILogger<FfmpegVideoService> _logger;

    public FfmpegVideoService(IAssetStorageService storage, ILogger<FfmpegVideoService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    // -------------------------------------------------------------------
    // IVideoAssemblyService — full FFmpeg slideshow pipeline (2D mode)
    // -------------------------------------------------------------------

    public async Task<string> AssembleVideoAsync(Episode episode)
    {
        var scenes = episode.Scenes.OrderBy(s => s.SceneNumber).ToList();
        var sceneClipPaths = new List<string>();
        var episodeDir = $"videos/ep{episode.Id}";

        for (var i = 0; i < scenes.Count; i++)
        {
            var scene = scenes[i];
            _logger.LogInformation("    [FFmpeg] Scene {Current}/{Total}: assembling clip...", i + 1, scenes.Count);
            var clipPath = await CreateSceneClipAsync(scene, episode.Id, episodeDir);
            sceneClipPaths.Add(clipPath);
        }

        return await ConcatClipsAsync(sceneClipPaths, episodeDir, episode.AudioMixPath);
    }

    // -------------------------------------------------------------------
    // Called by Kling path — just concat pre-made AI clips + mix music
    // -------------------------------------------------------------------

    public async Task<string> AssembleFromClipsAsync(List<string> clipRelPaths, Episode episode)
    {
        var episodeDir = $"videos/ep{episode.Id}";
        var scenes = episode.Scenes.OrderBy(s => s.SceneNumber).ToList();
        var processedClipPaths = new List<string>();

        // Ensure parity if arrays mismatched
        var count = Math.Min(clipRelPaths.Count, scenes.Count);

        for (var i = 0; i < count; i++)
        {
            var origClipRel = clipRelPaths[i];
            var origClipFull = Path.GetFullPath(_storage.GetFullPath(origClipRel));
            var scene = scenes[i];
            
            var audioFiles = scene.DialogueLines
                .Where(d => !string.IsNullOrEmpty(d.AudioPath))
                .OrderBy(d => d.LineOrder)
                .Select(d => Path.GetFullPath(_storage.GetFullPath(d.AudioPath!)))
                .Where(File.Exists)
                .ToList();

            var outRelPath = $"{episodeDir}/scene{scene.SceneNumber}_kling_with_audio.mp4";
            var outFullPath = Path.GetFullPath(_storage.GetFullPath(outRelPath));

            if (!File.Exists(outFullPath))
            {
                if (audioFiles.Count == 0)
                {
                    // No dialogue: Add silent audio track so concat works cleanly
                    _logger.LogInformation("    [FFmpeg] Scene {Num}: Adding silent audio track...", scene.SceneNumber);
                    await RunFfmpegAsync(
                        $"-i \"{origClipFull}\" -f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 " +
                        $"-c:v copy -c:a aac -shortest \"{outFullPath}\"");
                }
                else
                {
                    // Merge dialogue
                    _logger.LogInformation("    [FFmpeg] Scene {Num}: Merging {Count} dialogue lines...", scene.SceneNumber, audioFiles.Count);
                    var sceneAudioPath = Path.GetFullPath(_storage.GetFullPath($"{episodeDir}/scene{scene.SceneNumber}_kling_dialogue.mp3"));

                    if (audioFiles.Count > 1)
                    {
                        var inputArgs = string.Join(" ", audioFiles.Select(a => $"-i \"{a}\""));
                        await RunFfmpegAsync($"{inputArgs} -filter_complex \"concat=n={audioFiles.Count}:v=0:a=1[aout]\" -map \"[aout]\" \"{sceneAudioPath}\"");
                    }
                    else
                    {
                        File.Copy(audioFiles[0], sceneAudioPath, overwrite: true);
                    }

                    // apad + shortest ensures:
                    // - if audio is shorter than video, pad with silence until video ends
                    // - if audio is longer than video, cut audio when video ends
                    // This perfectly synchronizes the output length to the Kling video length without re-encoding video
                    await RunFfmpegAsync(
                        $"-i \"{origClipFull}\" -i \"{sceneAudioPath}\" " +
                        $"-filter_complex \"[1:a]apad[aout]\" -map 0:v:0 -map \"[aout]\" " +
                        $"-c:v copy -c:a aac -shortest \"{outFullPath}\"");
                }
            }
            else
            {
                _logger.LogInformation("    [FFmpeg] Scene {Num}: Cached clip with audio found → {Path}", scene.SceneNumber, outRelPath);
            }

            processedClipPaths.Add(outRelPath);
        }

        return await ConcatClipsAsync(processedClipPaths, episodeDir, episode.AudioMixPath);
    }

    // -------------------------------------------------------------------
    // Scene clip creation with Ken Burns zoom/pan + animated subtitles
    // -------------------------------------------------------------------

    private static readonly string[] KenBurnsMoves =
    [
        "zoompan=z='min(zoom+0.0008,1.3)':x='iw/2-(iw/zoom/2)':y='ih/2-(ih/zoom/2)'",       // zoom in center
        "zoompan=z='min(zoom+0.0008,1.3)':x='0':y='0'",                                        // zoom in top-left
        "zoompan=z='min(zoom+0.0008,1.3)':x='iw-(iw/zoom)':y='ih-(ih/zoom)'",                  // zoom in bottom-right
        "zoompan=z='if(lte(zoom,1.0),1.3,max(1.001,zoom-0.0008))':x='iw/2-(iw/zoom/2)':y='0'", // zoom out
        "zoompan=z='min(zoom+0.0006,1.2)':x='iw/2-(iw/zoom/2)+50*on/n':y='ih/2-(ih/zoom/2)'", // pan right
        "zoompan=z='min(zoom+0.0006,1.2)':x='iw/2-(iw/zoom/2)-50*on/n':y='ih/2-(ih/zoom/2)'", // pan left
    ];

    private async Task<string> CreateSceneClipAsync(Scene scene, int episodeId, string episodeDir)
    {
        var imagePath = Path.GetFullPath(_storage.GetFullPath(scene.ImagePath ?? ""));
        var clipPath = $"{episodeDir}/scene{scene.SceneNumber}.mp4";
        var fullClipPath = Path.GetFullPath(_storage.GetFullPath(clipPath));

        Directory.CreateDirectory(Path.GetDirectoryName(fullClipPath)!);

        var audioFiles = scene.DialogueLines
            .Where(d => !string.IsNullOrEmpty(d.AudioPath))
            .OrderBy(d => d.LineOrder)
            .Select(d => Path.GetFullPath(_storage.GetFullPath(d.AudioPath!)))
            .Where(File.Exists)
            .ToList();

        var duration = scene.DurationSeconds > 0 ? scene.DurationSeconds : 20;
        var fps = 25;
        var totalFrames = duration * fps;

        // Pick a Ken Burns move based on scene number for visual variety
        var kenBurns = KenBurnsMoves[(scene.SceneNumber - 1) % KenBurnsMoves.Length];
        var kenBurnsFilter = $"{kenBurns}:d={totalFrames}:s=1920x1080,fps={fps},format=yuv420p";

        // Build subtitle drawtext filters from dialogue lines
        var subtitleFilters = BuildSubtitleFilters(scene.DialogueLines.OrderBy(d => d.LineOrder).ToList(), duration);

        var videoFilter = subtitleFilters.Count > 0
            ? $"{kenBurnsFilter},{string.Join(",", subtitleFilters)}"
            : kenBurnsFilter;

        if (audioFiles.Count > 0)
        {
            var sceneAudioPath = Path.GetFullPath(_storage.GetFullPath($"{episodeDir}/scene{scene.SceneNumber}_audio.mp3"));

            if (audioFiles.Count > 1)
            {
                var inputArgs = string.Join(" ", audioFiles.Select(a => $"-i \"{a}\""));
                await RunFfmpegAsync($"{inputArgs} -filter_complex \"concat=n={audioFiles.Count}:v=0:a=1[aout]\" -map \"[aout]\" \"{sceneAudioPath}\"");
            }
            else
            {
                File.Copy(audioFiles[0], sceneAudioPath, overwrite: true);
            }

            await RunFfmpegAsync(
                $"-loop 1 -i \"{imagePath}\" -i \"{sceneAudioPath}\" " +
                $"-vf \"{videoFilter}\" " +
                $"-c:v libx264 -preset fast -crf 20 -c:a aac -shortest -t {duration} \"{fullClipPath}\"");
        }
        else
        {
            await RunFfmpegAsync(
                $"-loop 1 -i \"{imagePath}\" " +
                $"-vf \"{videoFilter}\" " +
                $"-c:v libx264 -preset fast -crf 20 -t {duration} \"{fullClipPath}\"");
        }

        return clipPath;
    }

    private static List<string> BuildSubtitleFilters(List<DialogueLine> lines, int sceneDuration)
    {
        if (lines.Count == 0) return [];

        var filters = new List<string>();
        var secondsPerLine = (double)sceneDuration / lines.Count;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var startSec = i * secondsPerLine;
            var endSec = (i + 1) * secondsPerLine;

            // Escape special characters for FFmpeg drawtext
            var text = (line.Text ?? "").Replace("'", "\\'").Replace(":", "\\:");
            var speaker = line.CharacterName.Replace("'", "\\'");

            // Character name (smaller, colored accent above)
            var nameColor = i % 2 == 0 ? "0xFF6B6B" : "0x6BAAFF"; // alternating colors per character
            filters.Add(
                $"drawtext=text='{speaker}':fontsize=28:fontcolor={nameColor}:fontfile=/Windows/Fonts/arialbd.ttf:" +
                $"box=1:boxcolor=black@0.6:boxborderw=6:" +
                $"x=(w-text_w)/2:y=h-100:" +
                $"enable='between(t,{startSec:F2},{endSec:F2})'");

            // Dialogue text (larger, white)
            filters.Add(
                $"drawtext=text='{text}':fontsize=36:fontcolor=white:fontfile=/Windows/Fonts/arial.ttf:" +
                $"box=1:boxcolor=black@0.55:boxborderw=8:" +
                $"x=(w-text_w)/2:y=h-60:" +
                $"enable='between(t,{startSec:F2},{endSec:F2})'");
        }

        return filters;
    }

    // -------------------------------------------------------------------
    // Concat clips + optionally layer background music at 20% volume
    // -------------------------------------------------------------------

    private async Task<string> ConcatClipsAsync(List<string> clipRelPaths, string episodeDir, string? musicRelPath)
    {
        var concatListPath = Path.GetFullPath(_storage.GetFullPath($"{episodeDir}/concat_list.txt"));
        var concatContent = new StringBuilder();
        foreach (var clip in clipRelPaths)
            concatContent.AppendLine($"file '{Path.GetFullPath(_storage.GetFullPath(clip))}'");
        await File.WriteAllTextAsync(concatListPath, concatContent.ToString());

        var finalPath = $"{episodeDir}/final.mp4";
        var fullFinalPath = Path.GetFullPath(_storage.GetFullPath(finalPath));

        if (!string.IsNullOrEmpty(musicRelPath))
        {
            var musicPath = Path.GetFullPath(_storage.GetFullPath(musicRelPath));
            var musicInfo = new FileInfo(musicPath);

            if (musicInfo.Exists && musicInfo.Length > 1024) // sanity: must be a real file
            {
                try
                {
                    // Mix background music at 20% volume under character voices
                    var concatNomusicPath = fullFinalPath.Replace("final.mp4", "final_nomusic.mp4");
                    await RunFfmpegAsync($"-f concat -safe 0 -i \"{concatListPath}\" -c copy \"{concatNomusicPath}\"");
                    await RunFfmpegAsync(
                        $"-i \"{concatNomusicPath}\" -i \"{musicPath}\" " +
                        $"-filter_complex \"[0:a]volume=1.0[voice];[1:a]volume=0.2,aloop=loop=-1:size=2e+09[music];[voice][music]amix=inputs=2:duration=first[aout]\" " +
                        $"-c:v copy -map 0:v -map \"[aout]\" -c:a aac \"{fullFinalPath}\"");
                    if (File.Exists(concatNomusicPath)) File.Delete(concatNomusicPath);
                    _logger.LogInformation("    [FFmpeg] Music mixed at 20% volume");
                    return finalPath;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("    [FFmpeg] Music mixing failed ({Error}) — falling back to no music", ex.Message);
                    // Clean up partial nomusic file if it exists
                    var noMusicPath = fullFinalPath.Replace("final.mp4", "final_nomusic.mp4");
                    if (File.Exists(noMusicPath)) File.Delete(noMusicPath);
                    // Fall through to produce a no-music final video
                }
            }
            else
            {
                _logger.LogWarning("    [FFmpeg] Music file missing or empty ({Path}) — skipping music mix", musicPath);
            }
        }

        await RunFfmpegAsync($"-f concat -safe 0 -i \"{concatListPath}\" -c copy \"{fullFinalPath}\"");
        return finalPath;
    }

    // -------------------------------------------------------------------
    // FFmpeg process runner
    // -------------------------------------------------------------------

    private static string? _resolvedFfmpegPath;

    private static string ResolveFfmpegPath()
    {
        if (_resolvedFfmpegPath != null) return _resolvedFfmpegPath;

        try
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg", Arguments = "-version",
                RedirectStandardOutput = true, RedirectStandardError = true,
                UseShellExecute = false, CreateNoWindow = true
            });
            p?.WaitForExit(3000);
            if (p?.ExitCode == 0)
                return _resolvedFfmpegPath = "ffmpeg";
        }
        catch { /* scan known install paths */ }

        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var candidates = new[]
        {
            Path.Combine(local, @"Microsoft\WinGet\Links\ffmpeg.exe"),
            @"C:\ProgramData\chocolatey\bin\ffmpeg.exe",
            @"C:\ffmpeg\bin\ffmpeg.exe",
        };

        var wingetPkgsDir = Path.Combine(local, @"Microsoft\WinGet\Packages");
        if (Directory.Exists(wingetPkgsDir))
            candidates = candidates.Concat(
                Directory.GetFiles(wingetPkgsDir, "ffmpeg.exe", SearchOption.AllDirectories)).ToArray();

        foreach (var path in candidates)
            if (File.Exists(path))
                return _resolvedFfmpegPath = path;

        return _resolvedFfmpegPath = "ffmpeg";
    }

    private async Task RunFfmpegAsync(string arguments)
    {
        var ffmpegPath = ResolveFfmpegPath();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-y {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            _logger.LogError("FFmpeg error: {Error}", stderr);
            throw new ExternalServiceException("FFmpeg", $"FFmpeg failed: {stderr}");
        }
    }
}
