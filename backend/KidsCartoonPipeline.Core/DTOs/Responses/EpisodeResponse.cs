namespace KidsCartoonPipeline.Core.DTOs.Responses;

public class EpisodeResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Moral { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public List<string>? SeoTags { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
    public string? YouTubeVideoId { get; set; }
    public string? YouTubeUrl { get; set; }
    public List<SceneResponse> Scenes { get; set; } = [];
    public List<CharacterResponse> Characters { get; set; } = [];
    public string? CurrentStageError { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
