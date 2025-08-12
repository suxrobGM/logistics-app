namespace Logistics.DriverApp.Services;

public interface ICache
{
    public void Set<T>(string key, T? value);
    public T? Get<T>(string key);
    public void Remove(string key);
}
