namespace KidsCartoonPipeline.Core.Entities;

public class YoutubeUpload
{
    public int Id { get; set; }
    public int EpisodeId { get; set; }
    public Episode? Episode { get; set; }
    public string? VideoId { get; set; }
    public string? Url { get; set; }
    public string? Status { get; set; }
    public long? Views { get; set; }
    public double? WatchTimeHours { get; set; }
    public decimal? EstimatedRevenue { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
