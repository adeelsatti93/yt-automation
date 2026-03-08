using KidsCartoonPipeline.Core.Enums;

namespace KidsCartoonPipeline.Core.Exceptions;

public class PipelineException : Exception
{
    public int EpisodeId { get; }
    public PipelineStage Stage { get; }

    public PipelineException(int episodeId, PipelineStage stage, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        EpisodeId = episodeId;
        Stage = stage;
    }
}
