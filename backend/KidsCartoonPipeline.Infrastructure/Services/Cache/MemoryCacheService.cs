using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace KidsCartoonPipeline.Infrastructure.Services.Cache;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = [];
    private readonly object _lock = new();

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public T? Get<T>(string key) => _cache.TryGetValue(key, out T? value) ? value : default;

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (ttl.HasValue)
            options.SetAbsoluteExpiration(ttl.Value);
        else
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(key, value, options);
        lock (_lock) { _keys.Add(key); }
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        lock (_lock) { _keys.Remove(key); }
    }

    public void RemoveByPrefix(string prefix)
    {
        List<string> keysToRemove;
        lock (_lock) { keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList(); }
        foreach (var key in keysToRemove)
            Remove(key);
    }

    public bool TryGet<T>(string key, out T? value) => _cache.TryGetValue(key, out value);
}
