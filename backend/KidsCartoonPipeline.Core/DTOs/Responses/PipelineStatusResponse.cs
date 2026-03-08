namespace KidsCartoonPipeline.Core.DTOs.Responses;

public class PipelineStatusResponse
{
    public bool IsActive { get; set; }
    public int ActiveEpisodes { get; set; }
    public int QueuedTopics { get; set; }
    public int PendingReview { get; set; }
    public DateTime? NextScheduledRun { get; set; }
    public List<PipelineJobResponse> RecentJobs { get; set; } = [];
}
