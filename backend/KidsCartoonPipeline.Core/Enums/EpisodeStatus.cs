namespace KidsCartoonPipeline.Core.Enums;

public enum EpisodeStatus
{
    TopicQueued = 0,
    GeneratingScript = 1,
    GeneratingImages = 2,
    GeneratingAudio = 3,
    GeneratingMusic = 4,
    RenderingVideo = 5,
    GeneratingSeo = 6,
    PendingReview = 7,
    Approved = 8,
    Uploading = 9,
    Scheduled = 10,
    Published = 11,
    Failed = 12,
    Rejected = 13
}
