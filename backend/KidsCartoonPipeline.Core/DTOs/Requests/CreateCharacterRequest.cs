namespace KidsCartoonPipeline.Core.DTOs.Requests;

public class CreateCharacterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? VoiceId { get; set; }
    public string? VoiceName { get; set; }
    public string? ImagePromptStyle { get; set; }
    public string? AvatarUrl { get; set; }
}
