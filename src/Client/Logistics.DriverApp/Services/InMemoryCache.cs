namespace Logistics.DriverApp.Services;

public class InMemoryCache : ICache
{
    private readonly Dictionary<string, object?> _cache = new();

    public void Set<T>(string key, T? value)
    {
        _cache.Add(key, value);
    }

    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return value == default ? default : (T)value;
        }

        return default;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
