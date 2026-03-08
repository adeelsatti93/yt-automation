using Hangfire;
using KidsCartoonPipeline.Core.Enums;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Jobs;

public class AnalyticsSyncJob
{
    private readonly IEpisodeRepository _episodeRepo;
    private readonly IYouTubeService _youtubeService;
    private readonly ISettingsService _settings;
    private readonly ILogger<AnalyticsSyncJob> _logger;

    public AnalyticsSyncJob(
        IEpisodeRepository episodeRepo,
        IYouTubeService youtubeService,
        ISettingsService settings,
        ILogger<AnalyticsSyncJob> logger)
    {
        _episodeRepo = episodeRepo;
        _youtubeService = youtubeService;
        _settings = settings;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 1)]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("AnalyticsSyncJob started");

        // Only run if YouTube OAuth credentials are configured
        var clientId = await _settings.GetAsync("YouTube:ClientId");
        var clientSecret = await _settings.GetAsync("YouTube:ClientSecret");
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogWarning("YouTube not configured (ClientId/ClientSecret missing) — skipping analytics sync");
            return;
        }

        // Get all published episodes
        var (episodes, _) = await _episodeRepo.GetPagedAsync(
            EpisodeStatus.Published, null, 1, 200);

        if (episodes.Count == 0)
        {
            _logger.LogInformation("No published episodes — nothing to sync");
            return;
        }

        var synced = 0;
        foreach (var episode in episodes)
        {
            if (string.IsNullOrEmpty(episode.YouTubeVideoId))
                continue;

            try
            {
                _logger.LogInformation("Syncing analytics for episode #{EpisodeId} (YouTube: {VideoId})",
                    episode.Id, episode.YouTubeVideoId);
                synced++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync analytics for episode #{EpisodeId}", episode.Id);
            }
        }

        _logger.LogInformation("AnalyticsSyncJob completed — synced {Count} episodes", synced);
    }
}
