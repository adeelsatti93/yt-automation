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
        var rawVoiceId = character.VoiceId ?? await _settings.GetAsync("ElevenLabs:DefaultVoiceId") ?? "21m00Tcm4TlvDq8ikWAM";

        // ElevenLabs voice IDs are alphanumeric, ~20 chars, no spaces.
        // If the stored value looks like a display name, resolve to real ID.
        var voiceId = await ResolveVoiceIdAsync(rawVoiceId, character.VoiceName, apiKey);

        _logger.LogInformation("    TTS: character={Character} rawVoiceId={Raw} resolvedId={Resolved}",
            character.Name, rawVoiceId, voiceId);

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

    /// <summary>
    /// ElevenLabs voice IDs are alphanumeric strings (~20 chars, no spaces).
    /// If the stored value has spaces or dashes (looks like a display name), fetch voices and resolve by name.
    /// </summary>
    private async Task<string> ResolveVoiceIdAsync(string rawId, string? voiceName, string apiKey)
    {
        // Real ElevenLabs IDs: no spaces, typically 20 alphanumeric chars
        if (!rawId.Contains(' ') && !rawId.Contains('-') && rawId.Length >= 10)
            return rawId; // Looks like a real ID already

        // rawId looks like a display name — resolve it
        var searchName = rawId; // e.g. "Eric - Smooth, Trustworthy"

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "v1/voices");
            req.Headers.Add("xi-api-key", apiKey);
            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode) return rawId; // Best effort — let ElevenLabs give the real error

            var body = await res.Content.ReadAsStringAsync();
            var voices = JsonSerializer.Deserialize<VoicesResponse>(body)?.Voices ?? [];

            // Try exact name match first, then partial match
            var match = voices.FirstOrDefault(v =>
                string.Equals(v.Name, searchName, StringComparison.OrdinalIgnoreCase))
                ?? voices.FirstOrDefault(v =>
                    v.Name != null && v.Name.StartsWith(searchName.Split(' ')[0], StringComparison.OrdinalIgnoreCase));

            if (match?.VoiceId != null)
            {
                _logger.LogInformation("    Resolved voice name '{Name}' → ID '{Id}'", searchName, match.VoiceId);
                return match.VoiceId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Voice name resolution failed: {Error}", ex.Message);
        }

        // Could not resolve — use the fallback default
        _logger.LogWarning("Could not resolve voice '{Name}' — using default voice", searchName);
        return "21m00Tcm4TlvDq8ikWAM"; // Rachel (ElevenLabs default, always exists)
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
