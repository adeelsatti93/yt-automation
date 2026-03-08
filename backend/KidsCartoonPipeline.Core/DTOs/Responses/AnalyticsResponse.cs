namespace KidsCartoonPipeline.Core.DTOs.Responses;

public class AnalyticsSummaryResponse
{
    public int TotalEpisodes { get; set; }
    public int Published { get; set; }
    public long TotalViews { get; set; }
    public double TotalWatchTimeHours { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AvgViewsPerEpisode { get; set; }
}

public class EpisodeAnalyticsResponse
{
    public int EpisodeId { get; set; }
    public string? YoutubeId { get; set; }
    public string? Title { get; set; }
    public long Views { get; set; }
    public double WatchTimeHours { get; set; }
    public decimal Revenue { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? YouTubeLink { get; set; }
}
