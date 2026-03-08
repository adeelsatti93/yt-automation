using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KidsCartoonPipeline.Infrastructure.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _db;

    public SettingsRepository(AppDbContext db) => _db = db;

    public async Task<List<AppSetting>> GetAllAsync()
        => await _db.AppSettings.OrderBy(s => s.Category).ThenBy(s => s.Key).ToListAsync();

    public async Task<AppSetting?> GetByKeyAsync(string key)
        => await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == key);

    public async Task<List<AppSetting>> GetByCategoryAsync(string category)
        => await _db.AppSettings.Where(s => s.Category == category).OrderBy(s => s.Key).ToListAsync();

    public async Task SaveAsync(AppSetting setting)
    {
        setting.UpdatedAt = DateTime.UtcNow;
        _db.AppSettings.Update(setting);
        await _db.SaveChangesAsync();
    }

    public async Task SaveBatchAsync(List<AppSetting> settings)
    {
        foreach (var setting in settings)
            setting.UpdatedAt = DateTime.UtcNow;
        _db.AppSettings.UpdateRange(settings);
        await _db.SaveChangesAsync();
    }

    public async Task UpsertAsync(string key, string? value)
    {
        var setting = await GetByKeyAsync(key);
        if (setting != null)
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
