using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Repositories;

public interface ITopicRepository
{
    Task<List<TopicSeed>> GetAllAsync();
    Task<TopicSeed?> GetByIdAsync(int id);
    Task<TopicSeed?> GetNextUnusedAsync();
    Task<TopicSeed> CreateAsync(TopicSeed topic);
    Task UpdateAsync(TopicSeed topic);
    Task DeleteAsync(int id);
    Task<int> GetUnusedCountAsync();
    Task UpdatePrioritiesAsync(List<(int Id, int Priority)> priorities);
}
