using KidsCartoonPipeline.Core.Enums;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IPipelineOrchestrator
{
    Task RunFullPipelineAsync(int episodeId);
    Task RunStageAsync(int episodeId, PipelineStage stage);
    Task ResumePipelineAsync(int episodeId, PipelineStage fromStage);
}
