namespace KidsCartoonPipeline.Core.DTOs.Responses;

public class PipelineJobResponse
{
    public int Id { get; set; }
    public int EpisodeId { get; set; }
    public string? EpisodeTitle { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? LogOutput { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? DurationSeconds { get; set; }
}
