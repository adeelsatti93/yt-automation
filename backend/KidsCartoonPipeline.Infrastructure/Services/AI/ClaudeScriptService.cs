using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Exceptions;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.AI;

public class ClaudeScriptService : IScriptGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settings;
    private readonly ILogger<ClaudeScriptService> _logger;

    public ClaudeScriptService(HttpClient httpClient, ISettingsService settings, ILogger<ClaudeScriptService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.anthropic.com/");
        _settings = settings;
        _logger = logger;
    }

    public async Task<Episode> GenerateScriptAsync(string topic, List<Character> characters, string? moral = null)
    {
        var apiKey = await _settings.GetApiKeyAsync("Anthropic");
        var promptTemplate = await _settings.GetAsync("Prompts:Script") ?? GetDefaultScriptPrompt();

        var charactersJson = JsonSerializer.Serialize(characters.Select(c => new { c.Name, c.Description }));
        var prompt = promptTemplate
            .Replace("{topic}", topic)
            .Replace("{characters_json}", charactersJson)
            .Replace("{moral}", moral ?? "Be kind and helpful");

        var request = new
        {
            model = "claude-sonnet-4-5",
            max_tokens = 4000,
            system = "You are a children's cartoon scriptwriter. Output ONLY valid JSON with no markdown, no code fences, no preamble.",
            messages = new[] { new { role = "user", content = prompt } }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/messages");
        httpRequest.Headers.Add("x-api-key", apiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new ExternalServiceException("Anthropic", $"Claude API error: {error}", (int)response.StatusCode);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseBody);
        var scriptJson = StripCodeFences(claudeResponse?.Content?.FirstOrDefault()?.Text
            ?? throw new ExternalServiceException("Anthropic", "Empty response from Claude"));

        var scriptData = JsonSerializer.Deserialize<ScriptOutput>(scriptJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new ExternalServiceException("Anthropic", "Failed to parse script JSON");

        var episode = new Episode
        {
            Title = scriptData.Title,
            Summary = scriptData.Summary,
            Moral = scriptData.Moral,
            Scenes = scriptData.Scenes.Select((s, i) => new Scene
            {
                SceneNumber = s.SceneNumber > 0 ? s.SceneNumber : i + 1,
                DurationSeconds = s.DurationSeconds > 0 ? s.DurationSeconds : 20,
                BackgroundDescription = s.Background,
                ActionDescription = s.Action,
                DialogueLines = s.Dialogue.Select((d, j) => new DialogueLine
                {
                    LineOrder = j + 1,
                    CharacterName = d.Character,
                    Text = d.Line,
                    Tone = d.Tone
                }).ToList()
            }).ToList()
        };

        _logger.LogInformation("Generated script '{Title}' with {SceneCount} scenes", episode.Title, episode.Scenes.Count);
        return episode;
    }

    public async Task<List<string>> GenerateTopicIdeasAsync(List<Character> characters, int count = 10)
    {
        var apiKey = await _settings.GetApiKeyAsync("Anthropic");
        var charactersDesc = string.Join(", ", characters.Select(c => $"{c.Name} ({c.Description})"));
        var prompt = $"Generate {count} creative episode topic ideas for a children's cartoon (ages 3-7). Characters: {charactersDesc}. Each topic should include a character name and a learning moment. Return ONLY a JSON array of strings.";

        var request = new
        {
            model = "claude-sonnet-4-5",
            max_tokens = 1000,
            system = "Output ONLY valid JSON arrays. No markdown, no code fences.",
            messages = new[] { new { role = "user", content = prompt } }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/messages");
        httpRequest.Headers.Add("x-api-key", apiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException("Anthropic", $"Claude API error: {responseBody}", (int)response.StatusCode);

        var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseBody);
        var text = StripCodeFences(claudeResponse?.Content?.FirstOrDefault()?.Text ?? "[]");
        return JsonSerializer.Deserialize<List<string>>(text) ?? [];
    }

    private static string GetDefaultScriptPrompt() => """
        You are an expert children's cartoon scriptwriter creating content for kids aged 3-7.
        Your scripts must be safe, positive, and educational with simple language (max 8 words per sentence).
        Topic: {topic}
        Characters: {characters_json}
        Target moral: {moral}
        Return ONLY valid JSON:
        {"title":"...","summary":"...","moral":"...","scenes":[{"scene_number":1,"duration_seconds":20,"background":"...","action":"...","dialogue":[{"character":"...","line":"...","tone":"..."}]}]}
        """;

    /// <summary>Strip markdown code fences (```json ... ```) that Claude sometimes adds despite prompting.</summary>
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
        [JsonPropertyName("content")]
        public List<ContentBlock>? Content { get; set; }
    }

    private class ContentBlock
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class ScriptOutput
    {
        [JsonPropertyName("title")] public string Title { get; set; } = "";
        [JsonPropertyName("summary")] public string Summary { get; set; } = "";
        [JsonPropertyName("moral")] public string Moral { get; set; } = "";
        [JsonPropertyName("scenes")] public List<SceneOutput> Scenes { get; set; } = [];
    }

    private class SceneOutput
    {
        [JsonPropertyName("scene_number")] public int SceneNumber { get; set; }
        [JsonPropertyName("duration_seconds")] public int DurationSeconds { get; set; }
        [JsonPropertyName("background")] public string Background { get; set; } = "";
        [JsonPropertyName("action")] public string Action { get; set; } = "";
        [JsonPropertyName("dialogue")] public List<DialogueOutput> Dialogue { get; set; } = [];
    }

    private class DialogueOutput
    {
        [JsonPropertyName("character")] public string Character { get; set; } = "";
        [JsonPropertyName("line")] public string Line { get; set; } = "";
        [JsonPropertyName("tone")] public string Tone { get; set; } = "";
    }
}
