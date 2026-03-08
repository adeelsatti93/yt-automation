namespace KidsCartoonPipeline.Core.DTOs.Responses;

public class SceneResponse
{
    public int Id { get; set; }
    public int SceneNumber { get; set; }
    public int DurationSeconds { get; set; }
    public string? BackgroundDescription { get; set; }
    public string? ActionDescription { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImagePromptUsed { get; set; }
    public List<DialogueLineResponse> DialogueLines { get; set; } = [];
}

public class DialogueLineResponse
{
    public int Id { get; set; }
    public int LineOrder { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Tone { get; set; }
    public string? AudioUrl { get; set; }
}
