using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Exceptions;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.Video;

/// <summary>
/// Animates scene images using Kling AI via Fal.ai queue REST API.
/// Images sent as base64 data URIs — no file upload step needed.
///
/// REST base: https://queue.fal.run
/// Model:     fal-ai/kling-video/v1/standard/image-to-video
/// Docs:      https://fal.ai/models/fal-ai/kling-video/v1/standard/image-to-video/api
/// </summary>
public class KlingAnimationService : IAnimationService
{
    private readonly HttpClient _http;
    private readonly IAssetStorageService _storage;
    private readonly ISettingsService _settings;
    private readonly ILogger<KlingAnimationService> _logger;

    private const string QueueBase  = "https://queue.fal.run";
    private const string ModelId    = "fal-ai/kling-video/v1/standard/image-to-video";
    private const string ModelFamily = "fal-ai/kling-video"; // used for status/result (shorter path per docs)

    public KlingAnimationService(
        HttpClient http,
        IAssetStorageService storage,
        ISettingsService settings,
        ILogger<KlingAnimationService> logger)
    {
        _http    = http;
        _storage = storage;
        _settings = settings;
        _logger  = logger;
    }

    public async Task<string> AnimateSceneAsync(
        Scene scene, int episodeId, string episodeDir, CancellationToken ct = default)
    {
        var apiKey = await _settings.GetRequiredAsync("Fal:ApiKey");

        var clipRelPath  = $"{episodeDir}/scene{scene.SceneNumber}_kling.mp4";
        var clipFullPath = Path.GetFullPath(_storage.GetFullPath(clipRelPath));
        Directory.CreateDirectory(Path.GetDirectoryName(clipFullPath)!);

        // Skip Kling API call if clip already exists (retries won't re-bill)
        if (File.Exists(clipFullPath))
        {
            _logger.LogInformation("    [Kling] Scene {SceneNum}: clip already exists, skipping API call → {Path}",
                scene.SceneNumber, clipRelPath);
            return clipRelPath;
        }

        // 1. Convert image to base64 data URI  (no upload needed — Fal.ai accepts data URIs natively)
        _logger.LogInformation("    [Kling] Scene {SceneNum}: encoding image...", scene.SceneNumber);
        var imageDataUri = await ToDataUriAsync(
            Path.GetFullPath(_storage.GetFullPath(scene.ImagePath!)), "image/png");

        // 2. Submit to queue
        _logger.LogInformation("    [Kling] Scene {SceneNum}: submitting to queue...", scene.SceneNumber);
        var requestId = await SubmitToQueueAsync(imageDataUri, scene.DurationSeconds, apiKey, ct);

        // 3. Poll until complete
        _logger.LogInformation("    [Kling] Scene {SceneNum}: polling (id={Id})...",
            scene.SceneNumber, requestId);
        var videoUrl = await PollForResultAsync(requestId, apiKey, ct);

        // 4. Download clip
        _logger.LogInformation("    [Kling] Scene {SceneNum}: downloading clip...", scene.SceneNumber);
        await DownloadFileAsync(videoUrl, clipFullPath, ct);

        _logger.LogInformation("    [Kling] Scene {SceneNum}: ✓ {Path}", scene.SceneNumber, clipRelPath);
        return clipRelPath;
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static async Task<string> ToDataUriAsync(string filePath, string mimeType)
    {
        var bytes = await File.ReadAllBytesAsync(filePath);
        return $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string url, string apiKey, HttpContent? content = null)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Key", apiKey);
        if (content != null)
            req.Content = content;
        return req;
    }

    // -----------------------------------------------------------------------
    // Queue Submit  — POST https://queue.fal.run/{model}
    // -----------------------------------------------------------------------

    private async Task<string> SubmitToQueueAsync(
        string imageDataUri, int durationSeconds, string apiKey, CancellationToken ct)
    {
        var duration = durationSeconds <= 7 ? "5" : "10";
        var payload  = new
        {
            image_url    = imageDataUri,
            duration     = duration,
            aspect_ratio = "16:9",
            prompt       = "Natural cartoon character movement, expressive facial animations, smooth motion, child-friendly style",
        };

        var body = JsonSerializer.Serialize(payload);
        using var req = BuildRequest(
            HttpMethod.Post,
            $"{QueueBase}/{ModelId}",
            apiKey,
            new StringContent(body, Encoding.UTF8, "application/json"));

        var res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync(ct);
            throw new ExternalServiceException("Kling", $"Submit failed ({res.StatusCode}): {err}");
        }

        var json = await res.Content.ReadFromJsonAsync<FalQueueResponse>(ct)
            ?? throw new ExternalServiceException("Kling", "Submit returned empty response");
        return json.RequestId
            ?? throw new ExternalServiceException("Kling", "Submit missing request_id");
    }

    // -----------------------------------------------------------------------
    // Poll Status  — GET https://queue.fal.run/{model}/requests/{id}/status
    // Get Result   — GET https://queue.fal.run/{model}/requests/{id}
    // -----------------------------------------------------------------------

    private async Task<string> PollForResultAsync(string requestId, string apiKey, CancellationToken ct)
    {
        // Status/result URLs use the shorter model family path (fal-ai/kling-video),
        // NOT the full versioned path — confirmed from official Fal.ai curl examples
        var statusUrl = $"{QueueBase}/{ModelFamily}/requests/{requestId}/status";
        var resultUrl = $"{QueueBase}/{ModelFamily}/requests/{requestId}";

        var timeout = DateTime.UtcNow.AddMinutes(10);
        var delay   = TimeSpan.FromSeconds(8);

        while (DateTime.UtcNow < timeout)
        {
            await Task.Delay(delay, ct);

            // Explicit auth on every request — avoids redirect auth-stripping issues
            using var statusReq = BuildRequest(HttpMethod.Get, statusUrl, apiKey);
            var statusRes = await _http.SendAsync(statusReq, ct);

            if (!statusRes.IsSuccessStatusCode)
            {
                _logger.LogWarning("    [Kling] Status non-200: {Code}", statusRes.StatusCode);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds + 2, 20));
                continue;
            }

            var status = await statusRes.Content.ReadFromJsonAsync<FalStatusResponse>(ct);
            _logger.LogInformation("    [Kling] Status: {Status}", status?.Status);

            switch (status?.Status)
            {
                case "COMPLETED":
                    using (var resultReq = BuildRequest(HttpMethod.Get, resultUrl, apiKey))
                    {
                        var resultRes = await _http.SendAsync(resultReq, ct);
                        resultRes.EnsureSuccessStatusCode();
                        var result = await resultRes.Content.ReadFromJsonAsync<KlingResult>(ct)
                            ?? throw new ExternalServiceException("Kling", "Result was empty");
                        return result.Video?.Url
                            ?? throw new ExternalServiceException("Kling", "Result missing video URL");
                    }

                case "FAILED":
                case "CANCELLED":
                    throw new ExternalServiceException("Kling",
                        $"Job {status.Status}: {status.Error}");
            }

            delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds + 2, 20));
        }

        throw new ExternalServiceException("Kling", $"Timed out (id={requestId})");
    }

    // -----------------------------------------------------------------------
    // Download
    // -----------------------------------------------------------------------

    private async Task DownloadFileAsync(string url, string destPath, CancellationToken ct)
    {
        var res = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        res.EnsureSuccessStatusCode();
        await using var fs = File.Create(destPath);
        await res.Content.CopyToAsync(fs, ct);
    }

    // -----------------------------------------------------------------------
    // DTOs
    // -----------------------------------------------------------------------

    private record FalQueueResponse(
        [property: JsonPropertyName("request_id")] string? RequestId,
        [property: JsonPropertyName("status")]     string? Status);

    private record FalStatusResponse(
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("error")]  string? Error);

    private record KlingResult(
        [property: JsonPropertyName("video")] KlingVideo? Video);

    private record KlingVideo(
        [property: JsonPropertyName("url")] string? Url);
}
