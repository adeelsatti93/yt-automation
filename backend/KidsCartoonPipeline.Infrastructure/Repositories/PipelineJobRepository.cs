using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Enums;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KidsCartoonPipeline.Infrastructure.Repositories;

public class PipelineJobRepository : IPipelineJobRepository
{
    private readonly AppDbContext _db;

    public PipelineJobRepository(AppDbContext db) => _db = db;

    public async Task<PipelineJob> CreateAsync(PipelineJob job)
    {
        job.CreatedAt = DateTime.UtcNow;
        _db.PipelineJobs.Add(job);
        await _db.SaveChangesAsync();
        return job;
    }

    public async Task UpdateAsync(PipelineJob job)
    {
        _db.PipelineJobs.Update(job);
        await _db.SaveChangesAsync();
    }

    public async Task<List<PipelineJob>> GetByEpisodeIdAsync(int episodeId)
        => await _db.PipelineJobs
            .Include(j => j.Episode)
            .Where(j => j.EpisodeId == episodeId)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync();

    public async Task<List<PipelineJob>> GetRecentAsync(int count = 20)
        => await _db.PipelineJobs
            .Include(j => j.Episode)
            .OrderByDescending(j => j.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<int> GetActiveCountAsync()
        => await _db.PipelineJobs.CountAsync(j => j.Status == JobStatus.Running);
}
