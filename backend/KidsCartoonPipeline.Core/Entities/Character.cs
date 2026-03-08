namespace KidsCartoonPipeline.Core.Entities;

public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? VoiceId { get; set; }
    public string? VoiceName { get; set; }
    public string? ImagePromptStyle { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Episode> Episodes { get; set; } = [];
}
