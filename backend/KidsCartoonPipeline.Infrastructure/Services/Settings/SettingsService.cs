using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.Settings;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repo;
    private readonly ICacheService _cache;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ISettingsRepository repo, ICacheService cache, ILogger<SettingsService> logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> GetAsync(string key)
    {
        var cacheKey = $"settings:{key}";
        if (_cache.TryGet<string>(cacheKey, out var cached))
            return cached;

        var setting = await _repo.GetByKeyAsync(key);
        if (setting?.Value != null)
            _cache.Set(cacheKey, setting.Value, TimeSpan.FromMinutes(5));
        return setting?.Value;
    }

    public async Task<string> GetRequiredAsync(string key)
    {
        var value = await GetAsync(key);
        if (string.IsNullOrEmpty(value))
            throw new InvalidOperationException($"Required setting '{key}' is not configured.");
        return value;
    }

    public async Task<string> GetApiKeyAsync(string service)
    {
        var key = $"{service}:ApiKey";
        var value = await GetAsync(key);
        if (string.IsNullOrEmpty(value))
            throw new InvalidOperationException($"API key for '{service}' is not configured. Please add it in Settings.");
        return value;
    }

    public async Task SetAsync(string key, string value)
    {
        var setting = await _repo.GetByKeyAsync(key);
        if (setting != null)
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveAsync(setting);
            _cache.Remove($"settings:{key}");
            _cache.Remove("settings:all");
            _logger.LogInformation("Setting '{Key}' updated", key);
        }
    }

    public async Task<List<AppSetting>> GetAllAsync()
    {
        if (_cache.TryGet<List<AppSetting>>("settings:all", out var cached) && cached != null)
            return cached;

        var settings = await _repo.GetAllAsync();
        _cache.Set("settings:all", settings, TimeSpan.FromMinutes(5));
        return settings;
    }

    public async Task<AppSetting?> GetSettingEntityAsync(string key)
    {
        var cacheKey = $"settings:entity:{key}";
        if (_cache.TryGet<AppSetting>(cacheKey, out var cached))
            return cached;

        var setting = await _repo.GetByKeyAsync(key);
        if (setting != null)
            _cache.Set(cacheKey, setting, TimeSpan.FromMinutes(5));
        return setting;
    }

    public async Task<List<AppSetting>> GetByCategoryAsync(string category)
        => await _repo.GetByCategoryAsync(category);

    public async Task SaveBatchAsync(List<(string Key, string Value)> settings)
    {
        foreach (var (key, value) in settings)
        {
            var setting = await _repo.GetByKeyAsync(key);
            if (setting != null)
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
                await _repo.SaveAsync(setting);
            }
        }
        _cache.RemoveByPrefix("settings:");
    }

    public async Task<(List<string> Configured, List<string> Missing, List<string> RequiredMissing)> GetConfigurationStatusAsync()
    {
        var apiKeys = await _repo.GetByCategoryAsync("ApiKeys");
        var configured = apiKeys.Where(s => !string.IsNullOrEmpty(s.Value)).Select(s => s.Key.Split(':')[0]).Distinct().ToList();
        var missing = apiKeys.Where(s => string.IsNullOrEmpty(s.Value)).Select(s => s.Key.Split(':')[0]).Distinct().ToList();
        var requiredMissing = apiKeys.Where(s => s.IsRequired && string.IsNullOrEmpty(s.Value)).Select(s => s.Key.Split(':')[0]).Distinct().ToList();
        return (configured, missing, requiredMissing);
    }

    public async Task<bool> TestApiKeyAsync(string service)
    {
        try
        {
            var apiKey = await GetApiKeyAsync(service);
            return !string.IsNullOrEmpty(apiKey);
        }
        catch
        {
            return false;
        }
    }
}
