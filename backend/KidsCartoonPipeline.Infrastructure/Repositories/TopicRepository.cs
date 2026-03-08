using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KidsCartoonPipeline.Infrastructure.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly AppDbContext _db;

    public TopicRepository(AppDbContext db) => _db = db;

    public async Task<List<TopicSeed>> GetAllAsync()
        => await _db.TopicSeeds
            .OrderByDescending(t => !t.IsUsed)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

    public async Task<TopicSeed?> GetByIdAsync(int id)
        => await _db.TopicSeeds.FindAsync(id);

    public async Task<TopicSeed?> GetNextUnusedAsync()
        => await _db.TopicSeeds
            .Where(t => !t.IsUsed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task<TopicSeed> CreateAsync(TopicSeed topic)
    {
        topic.CreatedAt = DateTime.UtcNow;
        _db.TopicSeeds.Add(topic);
        await _db.SaveChangesAsync();
        return topic;
    }

    public async Task UpdateAsync(TopicSeed topic)
    {
        _db.TopicSeeds.Update(topic);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var topic = await _db.TopicSeeds.FindAsync(id);
        if (topic != null)
        {
            // Cascade: delete linked episodes (and their scenes, jobs via their own cascades)
            var linkedEpisodes = await _db.Episodes
                .Where(e => e.TopicSeedId == id)
                .ToListAsync();
            _db.Episodes.RemoveRange(linkedEpisodes);

            _db.TopicSeeds.Remove(topic);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<int> GetUnusedCountAsync()
        => await _db.TopicSeeds.CountAsync(t => !t.IsUsed);

    public async Task UpdatePrioritiesAsync(List<(int Id, int Priority)> priorities)
    {
        foreach (var (id, priority) in priorities)
        {
            var topic = await _db.TopicSeeds.FindAsync(id);
            if (topic != null)
                topic.Priority = priority;
        }
        await _db.SaveChangesAsync();
    }
}
