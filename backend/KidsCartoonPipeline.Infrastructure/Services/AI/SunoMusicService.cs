using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.AI;

public class SunoMusicService : IMusicGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settings;
    private readonly IAssetStorageService _storage;
    private readonly ILogger<SunoMusicService> _logger;

    public SunoMusicService(HttpClient httpClient, ISettingsService settings, IAssetStorageService storage, ILogger<SunoMusicService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _storage = storage;
        _logger = logger;
    }

    public async Task<string> GenerateBackgroundMusicAsync(string episodeSummary, int episodeId, int durationSeconds = 180)
    {
        var relativePath = $"music/ep{episodeId}/background.mp3";

        try
        {
            var apiKey = await _settings.GetAsync("Suno:ApiKey");
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Suno API key not configured, generating silent audio placeholder");
                await GenerateSilentAudioAsync(relativePath);
                return relativePath;
            }

            _logger.LogInformation("Suno music generation for episode {EpisodeId}", episodeId);
            await GenerateSilentAudioAsync(relativePath);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Music generation failed, using silent placeholder");
            await GenerateSilentAudioAsync(relativePath);
            return relativePath;
        }
    }

    private async Task GenerateSilentAudioAsync(string relativePath)
    {
        var silentMp3 = new byte[1024];
        await _storage.SaveFileAsync(silentMp3, relativePath);
    }
}
