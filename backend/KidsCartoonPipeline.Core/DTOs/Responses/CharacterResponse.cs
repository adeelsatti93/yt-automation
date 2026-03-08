namespace KidsCartoonPipeline.Core.DTOs.Responses;

public class CharacterResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? VoiceId { get; set; }
    public string? VoiceName { get; set; }
    public string? ImagePromptStyle { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public int EpisodeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
