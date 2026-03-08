using KidsCartoonPipeline.Core.Enums;

namespace KidsCartoonPipeline.Core.Entities;

public class PipelineJob
{
    public int Id { get; set; }
    public int EpisodeId { get; set; }
    public Episode Episode { get; set; } = null!;
    public PipelineStage Stage { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public string? ErrorMessage { get; set; }
    public string? LogOutput { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
