using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Enums;

namespace KidsCartoonPipeline.Core.Interfaces.Repositories;

public interface IEpisodeRepository
{
    Task<Episode?> GetByIdAsync(int id);
    Task<Episode?> GetByIdWithDetailsAsync(int id);
    Task<(List<Episode> Items, int TotalCount)> GetPagedAsync(EpisodeStatus? status, string? search, int page, int pageSize);
    Task<Episode> CreateAsync(Episode episode);
    Task UpdateAsync(Episode episode);
    Task DeleteAsync(int id);
    Task<int> GetCountByStatusAsync(EpisodeStatus status);
    Task<List<Episode>> GetActiveEpisodesAsync();
}
