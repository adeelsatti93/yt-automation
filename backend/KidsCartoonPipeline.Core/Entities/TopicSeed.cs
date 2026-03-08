namespace KidsCartoonPipeline.Core.Entities;

public class TopicSeed
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetMoral { get; set; }
    public int Priority { get; set; } = 0;
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public int? EpisodeId { get; set; }
    public Episode? Episode { get; set; }
    public DateTime CreatedAt { get; set; }
}
