using System.Collections.Concurrent;
using Logistics.Models;

namespace Logistics.API.Hubs;

public class LiveTrackingHubContext
{
    private readonly ConcurrentDictionary<string, TruckGeolocationDto?> _connectedClients = new();

    public void AddClient(string connectionId, TruckGeolocationDto? geolocationData)
    {
        _connectedClients.TryAdd(connectionId, geolocationData);
    }

    public TruckGeolocationDto? GetGeolocationData(string connectionId)
    {
        _connectedClients.TryGetValue(connectionId, out var geolocationData);
        return geolocationData;
    }

    public void UpdateGeolocationData(string connectionId, TruckGeolocationDto truckGeolocationDto)
    {
        _connectedClients.AddOrUpdate(connectionId, truckGeolocationDto, (_, _) => truckGeolocationDto);
    }
    
    public void RemoveClient(string connectionId)
    {
        _connectedClients.TryRemove(connectionId, out _);
    }
}
