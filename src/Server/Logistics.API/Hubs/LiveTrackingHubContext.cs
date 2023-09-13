using System.Collections.Concurrent;
using Logistics.Models;

namespace Logistics.API.Hubs;

public class LiveTrackingHubContext
{
    private readonly ConcurrentDictionary<string, GeolocationData?> _connectedClients = new();

    public void AddClient(string connectionId, GeolocationData? geolocationData)
    {
        _connectedClients.TryAdd(connectionId, geolocationData);
    }

    public GeolocationData? GetGeolocationData(string connectionId)
    {
        _connectedClients.TryGetValue(connectionId, out var geolocationData);
        return geolocationData;
    }

    public void UpdateGeolocationData(string connectionId, GeolocationData geolocationData)
    {
        _connectedClients.AddOrUpdate(connectionId, geolocationData, (_, _) => geolocationData);
    }
    
    public void RemoveClient(string connectionId)
    {
        _connectedClients.TryRemove(connectionId, out _);
    }
}
