using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Exceptions;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.AI;

public class SeoGenerationService : ISeoGenerationService
{
    private readonly ISettingsService _settings;
    private readonly IImageGenerationService _imageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SeoGenerationService> _logger;

    public SeoGenerationService(ISettingsService settings, IImageGenerationService imageService, IHttpClientFactory httpClientFactory, ILogger<SeoGenerationService> logger)
    {
        _settings = settings;
        _imageService = imageService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SeoResult> GenerateSeoMetadataAsync(Episode episode, List<Character> characters)
    {
        var apiKey = await _settings.GetApiKeyAsync("Anthropic");
        var characterNames = string.Join(", ", characters.Select(c => c.Name));

        var prompt = $$"""
            Generate YouTube SEO metadata for a children's cartoon episode.
            Episode title: {{episode.Title}}
            Summary: {{episode.Summary}}
            Characters: {{characterNames}}
            Moral: {{episode.Moral}}
            Target audience: Parents searching for educational cartoons for kids aged 3-7
            Return ONLY valid JSON:
            {"seo_title":"Max 70 chars","seo_description":"300-500 words","tags":["15-20 tags"],"thumbnail_prompt":"Scene description for thumbnail"}
            """;

        var request = new
        {
            model = "claude-sonnet-4-5",
            max_tokens = 2000,
            system = "Output ONLY valid JSON. No markdown, no code fences.",
            messages = new[] { new { role = "user", content = prompt } }
        };

        var client = _httpClientFactory.CreateClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        httpRequest.Headers.Add("x-api-key", apiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new ExternalServiceException("Anthropic", $"SEO generation error: {error}", (int)response.StatusCode);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseBody);
        var text = StripCodeFences(claudeResponse?.Content?.FirstOrDefault()?.Text
            ?? throw new ExternalServiceException("Anthropic", "Empty SEO response"));

        var seoData = JsonSerializer.Deserialize<SeoOutput>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new ExternalServiceException("Anthropic", "Failed to parse SEO JSON");

        var thumbnailPath = await _imageService.GenerateThumbnailAsync(
            seoData.ThumbnailPrompt ?? episode.Title ?? "Kids cartoon thumbnail", episode.Id);

        _logger.LogInformation("Generated SEO metadata for episode {EpisodeId}", episode.Id);

        return new SeoResult
        {
            SeoTitle = seoData.SeoTitle ?? episode.Title ?? "",
            SeoDescription = seoData.SeoDescription ?? episode.Summary ?? "",
            Tags = seoData.Tags ?? [],
            ThumbnailPrompt = seoData.ThumbnailPrompt ?? ""
        };
    }

    private static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline > 0)
                trimmed = trimmed[(firstNewline + 1)..];
            if (trimmed.EndsWith("```"))
                trimmed = trimmed[..^3];
        }
        return trimmed.Trim();
    }

    private class ClaudeResponse
    {
        [JsonPropertyName("content")] public List<ContentBlock>? Content { get; set; }
    }
    private class ContentBlock
    {
        [JsonPropertyName("text")] public string? Text { get; set; }
    }
    private class SeoOutput
    {
        [JsonPropertyName("seo_title")] public string? SeoTitle { get; set; }
        [JsonPropertyName("seo_description")] public string? SeoDescription { get; set; }
        [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
        [JsonPropertyName("thumbnail_prompt")] public string? ThumbnailPrompt { get; set; }
    }
}
