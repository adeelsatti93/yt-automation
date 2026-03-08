using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Repositories;

public interface ISettingsRepository
{
    Task<List<AppSetting>> GetAllAsync();
    Task<AppSetting?> GetByKeyAsync(string key);
    Task<List<AppSetting>> GetByCategoryAsync(string category);
    Task SaveAsync(AppSetting setting);
    Task SaveBatchAsync(List<AppSetting> settings);
    Task UpsertAsync(string key, string? value);
}
