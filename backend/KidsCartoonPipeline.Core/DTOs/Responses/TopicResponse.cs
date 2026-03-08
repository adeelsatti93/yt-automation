namespace KidsCartoonPipeline.Core.DTOs.Responses;

public class TopicResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetMoral { get; set; }
    public int Priority { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public int? EpisodeId { get; set; }
    public DateTime CreatedAt { get; set; }
}
