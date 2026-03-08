using System.Diagnostics;
using System.Text.Json;
using Hangfire;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Enums;
using KidsCartoonPipeline.Core.Exceptions;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.Pipeline;

public class PipelineOrchestrator : IPipelineOrchestrator
{
    private readonly IEpisodeRepository _episodes;
    private readonly ICharacterRepository _characters;
    private readonly IPipelineJobRepository _jobs;
    private readonly IScriptGenerationService _scriptService;
    private readonly IImageGenerationService _imageService;
    private readonly IVoiceGenerationService _voiceService;
    private readonly IMusicGenerationService _musicService;
    private readonly IVideoAssemblyService _videoService;
    private readonly ISeoGenerationService _seoService;
    private readonly ILogger<PipelineOrchestrator> _logger;

    public PipelineOrchestrator(
        IEpisodeRepository episodes, ICharacterRepository characters, IPipelineJobRepository jobs,
        IScriptGenerationService scriptService, IImageGenerationService imageService,
        IVoiceGenerationService voiceService, IMusicGenerationService musicService,
        IVideoAssemblyService videoService, ISeoGenerationService seoService,
        ILogger<PipelineOrchestrator> logger)
    {
        _episodes = episodes;
        _characters = characters;
        _jobs = jobs;
        _scriptService = scriptService;
        _imageService = imageService;
        _voiceService = voiceService;
        _musicService = musicService;
        _videoService = videoService;
        _seoService = seoService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task RunFullPipelineAsync(int episodeId)
    {
        var pipelineSw = Stopwatch.StartNew();
        _logger.LogInformation("=== PIPELINE START === Episode {EpisodeId}", episodeId);

        var stages = new[]
        {
            PipelineStage.ScriptGeneration, PipelineStage.ImageGeneration,
            PipelineStage.VoiceGeneration, PipelineStage.MusicGeneration,
            PipelineStage.VideoAssembly, PipelineStage.SeoGeneration
        };

        foreach (var stage in stages)
        {
            try
            {
                await RunStageAsync(episodeId, stage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== PIPELINE FAILED === Episode {EpisodeId} at {Stage} after {Elapsed}", episodeId, stage, pipelineSw.Elapsed);
                var episode = await _episodes.GetByIdAsync(episodeId);
                if (episode != null)
                {
                    episode.Status = EpisodeStatus.Failed;
                    episode.CurrentStageError = $"{stage}: {ex.Message}";
                    await _episodes.UpdateAsync(episode);
                }
                throw;
            }
        }

        var ep = await _episodes.GetByIdAsync(episodeId);
        if (ep != null)
        {
            ep.Status = EpisodeStatus.PendingReview;
            await _episodes.UpdateAsync(ep);
        }
        pipelineSw.Stop();
        _logger.LogInformation("=== PIPELINE COMPLETE === Episode {EpisodeId} in {Elapsed}", episodeId, pipelineSw.Elapsed);
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task ResumePipelineAsync(int episodeId, PipelineStage fromStage)
    {
        var pipelineSw = Stopwatch.StartNew();
        _logger.LogInformation("=== PIPELINE RESUME === Episode {EpisodeId} from {Stage}", episodeId, fromStage);

        var allStages = new[]
        {
            PipelineStage.ScriptGeneration, PipelineStage.ImageGeneration,
            PipelineStage.VoiceGeneration, PipelineStage.MusicGeneration,
            PipelineStage.VideoAssembly, PipelineStage.SeoGeneration
        };

        var existingJobs = await _jobs.GetByEpisodeIdAsync(episodeId);
        var completedStages = existingJobs
            .Where(j => j.Status == JobStatus.Completed)
            .Select(j => j.Stage)
            .ToHashSet();

        _logger.LogInformation("Episode {EpisodeId}: {CompletedCount} stages already completed: [{Stages}]",
            episodeId, completedStages.Count, string.Join(", ", completedStages));

        var stages = allStages.SkipWhile(s => s != fromStage).ToArray();

        foreach (var stage in stages)
        {
            if (completedStages.Contains(stage))
            {
                _logger.LogInformation("  SKIP {Stage} — already completed", stage);
                continue;
            }

            try
            {
                await RunStageAsync(episodeId, stage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== PIPELINE FAILED === Episode {EpisodeId} at {Stage} after {Elapsed}", episodeId, stage, pipelineSw.Elapsed);
                var episode = await _episodes.GetByIdAsync(episodeId);
                if (episode != null)
                {
                    episode.Status = EpisodeStatus.Failed;
                    episode.CurrentStageError = $"{stage}: {ex.Message}";
                    await _episodes.UpdateAsync(episode);
                }
                throw;
            }
        }

        var ep = await _episodes.GetByIdAsync(episodeId);
        if (ep != null)
        {
            ep.Status = EpisodeStatus.PendingReview;
            await _episodes.UpdateAsync(ep);
        }
        pipelineSw.Stop();
        _logger.LogInformation("=== PIPELINE COMPLETE === Episode {EpisodeId} (resumed) in {Elapsed}", episodeId, pipelineSw.Elapsed);
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task RunStageAsync(int episodeId, PipelineStage stage)
    {
        var stageSw = Stopwatch.StartNew();
        _logger.LogInformation("  >> STAGE START: {Stage} for episode {EpisodeId}", stage, episodeId);

        var existing = await _episodes.GetByIdAsync(episodeId);
        if (existing == null)
        {
            _logger.LogWarning("  Episode {EpisodeId} not found — skipping {Stage}", episodeId, stage);
            return;
        }

        var job = await _jobs.CreateAsync(new PipelineJob
        {
            EpisodeId = episodeId, Stage = stage, Status = JobStatus.Running, StartedAt = DateTime.UtcNow
        });

        var episode = await _episodes.GetByIdWithDetailsAsync(episodeId)
            ?? throw new PipelineException(episodeId, stage, "Episode not found");
        var characters = episode.Characters.ToList();
        if (characters.Count == 0)
            characters = await _characters.GetActiveAsync();

        _logger.LogInformation("  Episode '{Title}' | {CharCount} characters | {SceneCount} scenes",
            episode.Title ?? "Untitled", characters.Count, episode.Scenes?.Count ?? 0);

        try
        {
            switch (stage)
            {
                case PipelineStage.ScriptGeneration:
                    episode.Status = EpisodeStatus.GeneratingScript;
                    await _episodes.UpdateAsync(episode);
                    _logger.LogInformation("    Calling Claude API for script generation...");
                    var scriptEpisode = await _scriptService.GenerateScriptAsync(
                        episode.TopicSeed?.Title ?? episode.Title ?? "A fun adventure",
                        characters, episode.TopicSeed?.TargetMoral ?? episode.Moral);
                    episode.Title = scriptEpisode.Title;
                    episode.Summary = scriptEpisode.Summary;
                    episode.Moral = scriptEpisode.Moral;
                    episode.Scenes = scriptEpisode.Scenes;
                    await _episodes.UpdateAsync(episode);
                    _logger.LogInformation("    Script generated: '{Title}' with {SceneCount} scenes, moral: '{Moral}'",
                        episode.Title, episode.Scenes.Count, episode.Moral);
                    break;

                case PipelineStage.ImageGeneration:
                    episode.Status = EpisodeStatus.GeneratingImages;
                    await _episodes.UpdateAsync(episode);
                    var sceneList = episode.Scenes.ToList();
                    for (var i = 0; i < sceneList.Count; i++)
                    {
                        var scene = sceneList[i];
                        _logger.LogInformation("    Generating image {Current}/{Total} (Scene {SceneNum})...",
                            i + 1, sceneList.Count, scene.SceneNumber);
                        scene.ImagePath = await _imageService.GenerateSceneImageAsync(scene, characters, episodeId);
                        _logger.LogInformation("    Image {Current}/{Total} saved: {Path}", i + 1, sceneList.Count, scene.ImagePath);
                    }
                    await _episodes.UpdateAsync(episode);
                    break;

                case PipelineStage.VoiceGeneration:
                    episode.Status = EpisodeStatus.GeneratingAudio;
                    await _episodes.UpdateAsync(episode);
                    var totalLines = episode.Scenes.Sum(s => s.DialogueLines.Count);
                    var lineNum = 0;
                    foreach (var scene in episode.Scenes)
                    {
                        foreach (var line in scene.DialogueLines)
                        {
                            lineNum++;
                            var character = characters.FirstOrDefault(c =>
                                c.Name.Equals(line.CharacterName, StringComparison.OrdinalIgnoreCase))
                                ?? characters.FirstOrDefault() ?? new Character { Name = "Narrator" };
                            _logger.LogInformation("    Generating voice {Current}/{Total}: {Character} says '{Text}'",
                                lineNum, totalLines, line.CharacterName, line.Text?.Length > 50 ? line.Text[..50] + "..." : line.Text);
                            line.AudioPath = await _voiceService.GenerateDialogueAudioAsync(line, character, episodeId);
                        }
                    }
                    await _episodes.UpdateAsync(episode);
                    _logger.LogInformation("    All {Total} dialogue lines voiced", totalLines);
                    break;

                case PipelineStage.MusicGeneration:
                    episode.Status = EpisodeStatus.GeneratingMusic;
                    await _episodes.UpdateAsync(episode);
                    var totalDuration = episode.Scenes.Sum(s => s.DurationSeconds);
                    var musicDuration = totalDuration > 0 ? totalDuration : 180;
                    _logger.LogInformation("    Generating background music ({Duration}s)...", musicDuration);
                    episode.AudioMixPath = await _musicService.GenerateBackgroundMusicAsync(
                        episode.Summary ?? "", episodeId, musicDuration);
                    await _episodes.UpdateAsync(episode);
                    _logger.LogInformation("    Music saved: {Path}", episode.AudioMixPath);
                    break;

                case PipelineStage.VideoAssembly:
                    episode.Status = EpisodeStatus.RenderingVideo;
                    await _episodes.UpdateAsync(episode);
                    _logger.LogInformation("    Assembling video with FFmpeg...");
                    episode.VideoPath = await _videoService.AssembleVideoAsync(episode);
                    await _episodes.UpdateAsync(episode);
                    _logger.LogInformation("    Video assembled: {Path}", episode.VideoPath);
                    break;

                case PipelineStage.SeoGeneration:
                    episode.Status = EpisodeStatus.GeneratingSeo;
                    await _episodes.UpdateAsync(episode);
                    _logger.LogInformation("    Generating SEO metadata + thumbnail...");
                    var seo = await _seoService.GenerateSeoMetadataAsync(episode, characters);
                    episode.SeoTitle = seo.SeoTitle;
                    episode.SeoDescription = seo.SeoDescription;
                    episode.SeoTags = JsonSerializer.Serialize(seo.Tags);
                    episode.ThumbnailPath = $"thumbnails/ep{episodeId}/thumbnail.png";
                    await _episodes.UpdateAsync(episode);
                    _logger.LogInformation("    SEO: '{SeoTitle}' | {TagCount} tags | Thumbnail generated",
                        seo.SeoTitle, seo.Tags?.Count ?? 0);
                    break;
            }

            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            await _jobs.UpdateAsync(job);

            stageSw.Stop();
            _logger.LogInformation("  << STAGE DONE: {Stage} for episode {EpisodeId} in {Elapsed}", stage, episodeId, stageSw.Elapsed);
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            await _jobs.UpdateAsync(job);

            stageSw.Stop();
            _logger.LogError(ex, "  << STAGE FAILED: {Stage} for episode {EpisodeId} after {Elapsed} — {Error}",
                stage, episodeId, stageSw.Elapsed, ex.Message);
            throw;
        }
    }
}
