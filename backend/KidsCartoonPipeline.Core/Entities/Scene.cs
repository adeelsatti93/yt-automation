namespace KidsCartoonPipeline.Core.Entities;

public class Scene
{
    public int Id { get; set; }
    public int EpisodeId { get; set; }
    public Episode Episode { get; set; } = null!;
    public int SceneNumber { get; set; }
    public int DurationSeconds { get; set; }
    public string? BackgroundDescription { get; set; }
    public string? ActionDescription { get; set; }
    public string? ImagePath { get; set; }
    public string? ImagePromptUsed { get; set; }
    public string? VideoClipPath { get; set; }
    public ICollection<DialogueLine> DialogueLines { get; set; } = [];
}
