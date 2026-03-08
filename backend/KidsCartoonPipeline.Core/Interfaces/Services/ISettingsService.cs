using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface ISettingsService
{
    Task<string?> GetAsync(string key);
    Task<string> GetRequiredAsync(string key);
    Task<string> GetApiKeyAsync(string service);
    Task SetAsync(string key, string value);
    Task<List<AppSetting>> GetAllAsync();
    Task<List<AppSetting>> GetByCategoryAsync(string category);
    Task SaveBatchAsync(List<(string Key, string Value)> settings);
    Task<AppSetting?> GetSettingEntityAsync(string key);
    Task<(List<string> Configured, List<string> Missing, List<string> RequiredMissing)> GetConfigurationStatusAsync();
    Task<bool> TestApiKeyAsync(string service);
}
