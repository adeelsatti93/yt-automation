using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Exceptions;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.AI;

public class DalleImageService : IImageGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settings;
    private readonly IAssetStorageService _storage;
    private readonly ILogger<DalleImageService> _logger;

    private const int MaxRetries = 3;

    public DalleImageService(HttpClient httpClient, ISettingsService settings, IAssetStorageService storage, ILogger<DalleImageService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.openai.com/");
        _settings = settings;
        _storage = storage;
        _logger = logger;
    }

    public async Task<string> GenerateSceneImageAsync(Scene scene, List<Character> characters, int episodeId)
    {
        var apiKey = await _settings.GetApiKeyAsync("OpenAI");
        var globalStyle = await _settings.GetAsync("Images:GlobalStyle") ?? "2D flat cartoon, bright saturated colors, child-friendly";
        var quality = await _settings.GetAsync("Images:Quality") ?? "hd";
        var size = await _settings.GetAsync("Images:Size") ?? "1792x1024";

        var characterDescs = string.Join("; ", characters.Select(c => $"{c.Name}: {c.Description} {c.ImagePromptStyle}"));
        var imagePrompt = $"{globalStyle}. {scene.BackgroundDescription}. {scene.ActionDescription}. Characters: {characterDescs}. No text, no watermarks, 16:9 aspect ratio.";
        if (imagePrompt.Length > 4000) imagePrompt = imagePrompt[..4000];

        var request = new { model = "dall-e-3", prompt = imagePrompt, n = 1, size, quality, response_format = "b64_json" };

        var b64Data = await CallDalleWithRetryAsync(apiKey, request, $"scene {scene.SceneNumber}");

        var imageBytes = Convert.FromBase64String(b64Data);
        var relativePath = $"images/ep{episodeId}/scene{scene.SceneNumber}.png";
        await _storage.SaveFileAsync(imageBytes, relativePath);

        _logger.LogInformation("Generated scene image for episode {EpisodeId} scene {SceneNumber}", episodeId, scene.SceneNumber);
        return relativePath;
    }

    public async Task<string> GenerateThumbnailAsync(string prompt, int episodeId)
    {
        var apiKey = await _settings.GetApiKeyAsync("OpenAI");
        var globalStyle = await _settings.GetAsync("Images:GlobalStyle") ?? "2D flat cartoon, bright saturated colors";
        var fullPrompt = $"{globalStyle}. {prompt}. Vibrant, eye-catching thumbnail. No text, no watermarks.";

        var request = new { model = "dall-e-3", prompt = fullPrompt, n = 1, size = "1792x1024", quality = "hd", response_format = "b64_json" };

        var b64Data = await CallDalleWithRetryAsync(apiKey, request, "thumbnail");

        var imageBytes = Convert.FromBase64String(b64Data);
        var relativePath = $"thumbnails/ep{episodeId}/thumbnail.png";
        await _storage.SaveFileAsync(imageBytes, relativePath);
        return relativePath;
    }

    private async Task<string> CallDalleWithRetryAsync(string apiKey, object request, string context)
    {
        var jsonContent = JsonSerializer.Serialize(request);

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var response = await _httpClient.PostAsync("v1/images/generations",
                new StringContent(jsonContent, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var dalleResponse = JsonSerializer.Deserialize<DalleResponse>(responseBody);
                return dalleResponse?.Data?.FirstOrDefault()?.B64Json
                    ?? throw new ExternalServiceException("OpenAI", $"Empty image response from DALL-E ({context})");
            }

            var error = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;
            var isRetryable = response.StatusCode == HttpStatusCode.TooManyRequests
                           || response.StatusCode == HttpStatusCode.InternalServerError
                           || response.StatusCode == HttpStatusCode.BadGateway
                           || response.StatusCode == HttpStatusCode.ServiceUnavailable
                           || response.StatusCode == HttpStatusCode.GatewayTimeout;

            if (!isRetryable || attempt == MaxRetries)
            {
                _logger.LogError("DALL-E {Context} failed (attempt {Attempt}/{Max}, status {Status}): {Error}",
                    context, attempt, MaxRetries, statusCode, error);
                throw new ExternalServiceException("OpenAI", $"DALL-E API error: {error}", statusCode);
            }

            var delay = attempt * 5; // 5s, 10s, 15s
            _logger.LogWarning("DALL-E {Context} returned {Status} (attempt {Attempt}/{Max}), retrying in {Delay}s...",
                context, statusCode, attempt, MaxRetries, delay);
            await Task.Delay(TimeSpan.FromSeconds(delay));
        }

        throw new ExternalServiceException("OpenAI", "DALL-E max retries exceeded");
    }

    private class DalleResponse
    {
        [JsonPropertyName("data")] public List<DalleImage>? Data { get; set; }
    }
    private class DalleImage
    {
        [JsonPropertyName("b64_json")] public string? B64Json { get; set; }
    }
}
