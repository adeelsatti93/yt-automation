using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KidsCartoonPipeline.Infrastructure.Repositories;

public class CharacterRepository : ICharacterRepository
{
    private readonly AppDbContext _db;

    public CharacterRepository(AppDbContext db) => _db = db;

    public async Task<List<Character>> GetAllAsync()
        => await _db.Characters
            .Include(c => c.Episodes)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<List<Character>> GetActiveAsync()
        => await _db.Characters
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<Character?> GetByIdAsync(int id)
        => await _db.Characters
            .Include(c => c.Episodes)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Character> CreateAsync(Character character)
    {
        character.CreatedAt = DateTime.UtcNow;
        character.UpdatedAt = DateTime.UtcNow;
        _db.Characters.Add(character);
        await _db.SaveChangesAsync();
        return character;
    }

    public async Task UpdateAsync(Character character)
    {
        character.UpdatedAt = DateTime.UtcNow;
        _db.Characters.Update(character);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var character = await _db.Characters.FindAsync(id);
        if (character != null)
        {
            _db.Characters.Remove(character);
            await _db.SaveChangesAsync();
        }
    }
}
