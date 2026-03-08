using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Repositories;

public interface ICharacterRepository
{
    Task<List<Character>> GetAllAsync();
    Task<List<Character>> GetActiveAsync();
    Task<Character?> GetByIdAsync(int id);
    Task<Character> CreateAsync(Character character);
    Task UpdateAsync(Character character);
    Task DeleteAsync(int id);
}
