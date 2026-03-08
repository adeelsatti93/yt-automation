using KidsCartoonPipeline.Core.Enums;

namespace KidsCartoonPipeline.Core.Entities;

public class Episode
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Moral { get; set; }
    public EpisodeStatus Status { get; set; } = EpisodeStatus.TopicQueued;
    public int? TopicSeedId { get; set; }
    public TopicSeed? TopicSeed { get; set; }
    public string? VideoPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? AudioMixPath { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoTags { get; set; }
    public string? YouTubeVideoId { get; set; }
    public string? YouTubeUrl { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? CurrentStageError { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Scene> Scenes { get; set; } = [];
    public ICollection<PipelineJob> PipelineJobs { get; set; } = [];
    public ICollection<Character> Characters { get; set; } = [];
}
