using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Repositories;

public interface IPipelineJobRepository
{
    Task<PipelineJob> CreateAsync(PipelineJob job);
    Task UpdateAsync(PipelineJob job);
    Task<List<PipelineJob>> GetByEpisodeIdAsync(int episodeId);
    Task<List<PipelineJob>> GetRecentAsync(int count = 20);
    Task<int> GetActiveCountAsync();
}
