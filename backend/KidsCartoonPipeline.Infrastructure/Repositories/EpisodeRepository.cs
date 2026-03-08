using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Enums;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KidsCartoonPipeline.Infrastructure.Repositories;

public class EpisodeRepository : IEpisodeRepository
{
    private readonly AppDbContext _db;

    public EpisodeRepository(AppDbContext db) => _db = db;

    public async Task<Episode?> GetByIdAsync(int id)
        => await _db.Episodes.FindAsync(id);

    public async Task<Episode?> GetByIdWithDetailsAsync(int id)
        => await _db.Episodes
            .Include(e => e.Scenes).ThenInclude(s => s.DialogueLines)
            .Include(e => e.Characters)
            .Include(e => e.PipelineJobs)
            .Include(e => e.TopicSeed)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<(List<Episode> Items, int TotalCount)> GetPagedAsync(
        EpisodeStatus? status, string? search, int page, int pageSize)
    {
        var query = _db.Episodes
            .Include(e => e.Characters)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Title != null && e.Title.Contains(search));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Episode> CreateAsync(Episode episode)
    {
        episode.CreatedAt = DateTime.UtcNow;
        episode.UpdatedAt = DateTime.UtcNow;
        _db.Episodes.Add(episode);
        await _db.SaveChangesAsync();
        return episode;
    }

    public async Task UpdateAsync(Episode episode)
    {
        episode.UpdatedAt = DateTime.UtcNow;
        if (_db.Entry(episode).State == EntityState.Detached)
            _db.Episodes.Update(episode);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var episode = await _db.Episodes.FindAsync(id);
        if (episode != null)
        {
            _db.Episodes.Remove(episode);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<int> GetCountByStatusAsync(EpisodeStatus status)
        => await _db.Episodes.CountAsync(e => e.Status == status);

    public async Task<List<Episode>> GetActiveEpisodesAsync()
        => await _db.Episodes
            .Where(e => e.Status != EpisodeStatus.PendingReview
                     && e.Status != EpisodeStatus.Published
                     && e.Status != EpisodeStatus.Failed
                     && e.Status != EpisodeStatus.Rejected
                     && e.Status != EpisodeStatus.TopicQueued
                     && e.Status != EpisodeStatus.Approved
                     && e.Status != EpisodeStatus.Scheduled)
            .ToListAsync();
}
