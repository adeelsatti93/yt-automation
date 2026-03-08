using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Exceptions;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.AI;

public class ElevenLabsVoiceService : IVoiceGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settings;
    private readonly IAssetStorageService _storage;
    private readonly ILogger<ElevenLabsVoiceService> _logger;

    public ElevenLabsVoiceService(HttpClient httpClient, ISettingsService settings, IAssetStorageService storage, ILogger<ElevenLabsVoiceService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.elevenlabs.io/");
        _settings = settings;
        _storage = storage;
        _logger = logger;
    }

    public async Task<string> GenerateDialogueAudioAsync(DialogueLine line, Character character, int episodeId)
    {
        var apiKey = await _settings.GetApiKeyAsync("ElevenLabs");
        var voiceId = character.VoiceId ?? await _settings.GetAsync("ElevenLabs:DefaultVoiceId") ?? "21m00Tcm4TlvDq8ikWAM";

        var request = new
        {
            text = line.Text,
            model_id = "eleven_turbo_v2",
            voice_settings = new { stability = 0.75, similarity_boost = 0.85, style = 0.3, use_speaker_boost = true }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"v1/text-to-speech/{voiceId}");
        httpRequest.Headers.Add("xi-api-key", apiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new ExternalServiceException("ElevenLabs", $"TTS API error: {error}", (int)response.StatusCode);
        }

        var audioBytes = await response.Content.ReadAsByteArrayAsync();
        var relativePath = $"audio/ep{episodeId}/scene{line.SceneId}_line{line.LineOrder}.mp3";
        await _storage.SaveFileAsync(audioBytes, relativePath);

        _logger.LogInformation("Generated audio for episode {EpisodeId}, line {LineOrder}", episodeId, line.LineOrder);
        return relativePath;
    }

    public async Task<List<VoiceInfo>> GetAvailableVoicesAsync()
    {
        var apiKey = await _settings.GetApiKeyAsync("ElevenLabs");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "v1/voices");
        httpRequest.Headers.Add("xi-api-key", apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException("ElevenLabs", "Failed to fetch voices");

        var body = await response.Content.ReadAsStringAsync();
        var voicesResponse = JsonSerializer.Deserialize<VoicesResponse>(body);

        return voicesResponse?.Voices?.Select(v => new VoiceInfo
        {
            VoiceId = v.VoiceId ?? "",
            Name = v.Name ?? "",
            PreviewUrl = v.PreviewUrl,
            Category = v.Category
        }).ToList() ?? [];
    }

    public async Task<string> GenerateTestAudioAsync(string voiceId, string text)
    {
        var apiKey = await _settings.GetApiKeyAsync("ElevenLabs");
        var request = new
        {
            text = text.Length > 0 ? text : "Hello! I'm a friendly character!",
            model_id = "eleven_turbo_v2",
            voice_settings = new { stability = 0.75, similarity_boost = 0.85 }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"v1/text-to-speech/{voiceId}");
        httpRequest.Headers.Add("xi-api-key", apiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new ExternalServiceException("ElevenLabs", $"Test voice generation failed: {error}", (int)response.StatusCode);
        }

        var audioBytes = await response.Content.ReadAsByteArrayAsync();
        var relativePath = $"audio/test/{voiceId}_{DateTime.UtcNow.Ticks}.mp3";
        await _storage.SaveFileAsync(audioBytes, relativePath);
        return relativePath;
    }

    private class VoicesResponse
    {
        [JsonPropertyName("voices")] public List<VoiceItem>? Voices { get; set; }
    }
    private class VoiceItem
    {
        [JsonPropertyName("voice_id")] public string? VoiceId { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("preview_url")] public string? PreviewUrl { get; set; }
        [JsonPropertyName("category")] public string? Category { get; set; }
    }
}
