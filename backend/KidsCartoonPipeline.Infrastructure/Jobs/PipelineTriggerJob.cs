using Hangfire;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Enums;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Jobs;

public class PipelineTriggerJob
{
    private readonly ISettingsService _settings;
    private readonly ITopicRepository _topicRepo;
    private readonly IEpisodeRepository _episodeRepo;
    private readonly ICharacterRepository _characterRepo;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly ILogger<PipelineTriggerJob> _logger;

    public PipelineTriggerJob(
        ISettingsService settings,
        ITopicRepository topicRepo,
        IEpisodeRepository episodeRepo,
        ICharacterRepository characterRepo,
        IPipelineOrchestrator orchestrator,
        ILogger<PipelineTriggerJob> logger)
    {
        _settings = settings;
        _topicRepo = topicRepo;
        _episodeRepo = episodeRepo;
        _characterRepo = characterRepo;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("PipelineTriggerJob started");

        // Check if pipeline is active
        var isActive = await _settings.GetAsync("Pipeline:IsActive");
        if (!string.Equals(isActive, "true", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Pipeline is paused — skipping trigger");
            return;
        }

        // Check capacity
        var maxConcurrentStr = await _settings.GetAsync("Pipeline:MaxConcurrent") ?? "1";
        var maxConcurrent = int.TryParse(maxConcurrentStr, out var mc) ? mc : 1;
        var activeEpisodes = await _episodeRepo.GetActiveEpisodesAsync();
        if (activeEpisodes.Count >= maxConcurrent)
        {
            _logger.LogInformation("Max concurrent episodes reached ({Active}/{Max}) — skipping",
                activeEpisodes.Count, maxConcurrent);
            return;
        }

        // Pick next topic
        var topic = await _topicRepo.GetNextUnusedAsync();
        if (topic == null)
        {
            _logger.LogWarning("No unused topics available — skipping pipeline trigger");
            return;
        }

        // Get active characters
        var characters = await _characterRepo.GetActiveAsync();
        if (characters.Count == 0)
        {
            _logger.LogWarning("No active characters configured — skipping pipeline trigger");
            return;
        }

        // Create episode from topic
        var episode = new Episode
        {
            Title = topic.Title,
            TopicSeedId = topic.Id,
            Status = EpisodeStatus.TopicQueued,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Characters = characters,
        };

        var created = await _episodeRepo.CreateAsync(episode);
        _logger.LogInformation("Created episode #{EpisodeId} from topic '{TopicTitle}'",
            created.Id, topic.Title);

        // Mark topic as used
        topic.IsUsed = true;
        topic.UsedAt = DateTime.UtcNow;
        topic.EpisodeId = created.Id;
        await _topicRepo.UpdateAsync(topic);

        // Enqueue full pipeline
        BackgroundJob.Enqueue<IPipelineOrchestrator>(o => o.RunFullPipelineAsync(created.Id));
        _logger.LogInformation("Enqueued full pipeline for episode #{EpisodeId}", created.Id);
    }
}
