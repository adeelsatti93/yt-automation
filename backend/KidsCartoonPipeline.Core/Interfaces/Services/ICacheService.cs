namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    void Remove(string key);
    void RemoveByPrefix(string prefix);
    bool TryGet<T>(string key, out T? value);
}
