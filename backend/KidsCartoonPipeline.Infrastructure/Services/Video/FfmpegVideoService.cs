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

    public async Task<string> AssembleVideoAsync(Episode episode)
    {
        var scenes = episode.Scenes.OrderBy(s => s.SceneNumber).ToList();
        var sceneClipPaths = new List<string>();
        var episodeDir = $"videos/ep{episode.Id}";

        foreach (var scene in scenes)
        {
            var clipPath = await CreateSceneClipAsync(scene, episode.Id, episodeDir);
            sceneClipPaths.Add(clipPath);
        }

        var concatListPath = Path.GetFullPath(_storage.GetFullPath($"{episodeDir}/concat_list.txt"));
        var concatContent = new StringBuilder();
        foreach (var clip in sceneClipPaths)
            concatContent.AppendLine($"file '{Path.GetFullPath(_storage.GetFullPath(clip))}'");
        await File.WriteAllTextAsync(concatListPath, concatContent.ToString());

        var finalPath = $"{episodeDir}/final.mp4";
        var fullFinalPath = Path.GetFullPath(_storage.GetFullPath(finalPath));

        await RunFfmpegAsync($"-f concat -safe 0 -i \"{concatListPath}\" -c copy \"{fullFinalPath}\"");

        _logger.LogInformation("Assembled video for episode {EpisodeId}: {Path}", episode.Id, finalPath);
        return finalPath;
    }

    private async Task<string> CreateSceneClipAsync(Scene scene, int episodeId, string episodeDir)
    {
        var imagePath = _storage.GetFullPath(scene.ImagePath ?? "");
        var clipPath = $"{episodeDir}/scene{scene.SceneNumber}.mp4";
        var fullClipPath = _storage.GetFullPath(clipPath);

        var dir = Path.GetDirectoryName(fullClipPath);
        if (dir != null) Directory.CreateDirectory(dir);

        var audioFiles = scene.DialogueLines
            .Where(d => !string.IsNullOrEmpty(d.AudioPath))
            .OrderBy(d => d.LineOrder)
            .Select(d => _storage.GetFullPath(d.AudioPath!))
            .Where(File.Exists)
            .ToList();

        var duration = scene.DurationSeconds > 0 ? scene.DurationSeconds : 20;

        if (audioFiles.Count > 0)
        {
            var sceneAudioPath = _storage.GetFullPath($"{episodeDir}/scene{scene.SceneNumber}_audio.mp3");

            if (audioFiles.Count > 1)
            {
                var inputArgs = string.Join(" ", audioFiles.Select(a => $"-i \"{a}\""));
                await RunFfmpegAsync($"{inputArgs} -filter_complex \"concat=n={audioFiles.Count}:v=0:a=1\" \"{sceneAudioPath}\"");
            }
            else
            {
                File.Copy(audioFiles[0], sceneAudioPath, overwrite: true);
            }

            await RunFfmpegAsync($"-loop 1 -i \"{imagePath}\" -i \"{sceneAudioPath}\" -vf \"scale=1920:1080:force_original_aspect_ratio=decrease,pad=1920:1080:(ow-iw)/2:(oh-ih)/2\" -c:v libx264 -preset fast -crf 20 -c:a aac -shortest -t {duration} \"{fullClipPath}\"");
        }
        else
        {
            await RunFfmpegAsync($"-loop 1 -i \"{imagePath}\" -vf \"scale=1920:1080:force_original_aspect_ratio=decrease,pad=1920:1080:(ow-iw)/2:(oh-ih)/2\" -c:v libx264 -preset fast -crf 20 -t {duration} -pix_fmt yuv420p \"{fullClipPath}\"");
        }

        return clipPath;
    }

    private static string? _resolvedFfmpegPath;

    private static string ResolveFfmpegPath()
    {
        if (_resolvedFfmpegPath != null) return _resolvedFfmpegPath;

        // 1. Check if ffmpeg is on PATH
        try
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg", Arguments = "-version",
                RedirectStandardOutput = true, RedirectStandardError = true,
                UseShellExecute = false, CreateNoWindow = true
            });
            p?.WaitForExit(3000);
            p?.Kill();
            if (p?.ExitCode == 0)
            {
                _resolvedFfmpegPath = "ffmpeg";
                return _resolvedFfmpegPath;
            }
        }
        catch { /* not on PATH, try common locations */ }

        // 2. Check common install paths
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var candidates = new[]
        {
            Path.Combine(userProfile, @"Microsoft\WinGet\Links\ffmpeg.exe"),
            @"C:\ProgramData\chocolatey\bin\ffmpeg.exe",
            @"C:\ffmpeg\bin\ffmpeg.exe",
        };

        // Also scan winget packages directory for any ffmpeg installation
        var wingetPkgsDir = Path.Combine(userProfile, @"Microsoft\WinGet\Packages");
        if (Directory.Exists(wingetPkgsDir))
        {
            var ffmpegExes = Directory.GetFiles(wingetPkgsDir, "ffmpeg.exe", SearchOption.AllDirectories);
            candidates = candidates.Concat(ffmpegExes).ToArray();
        }

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                _resolvedFfmpegPath = path;
                return _resolvedFfmpegPath;
            }
        }

        // Fallback — let it fail with a clear message
        _resolvedFfmpegPath = "ffmpeg";
        return _resolvedFfmpegPath;
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
