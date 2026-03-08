namespace KidsCartoonPipeline.Core.DTOs.Requests;

public class UpdateEpisodeMetadataRequest
{
    public string? Title { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoTags { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
}
