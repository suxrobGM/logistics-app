using System.Collections.Concurrent;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

public class TrackingHubContext
{
    private readonly ConcurrentDictionary<string, TruckGeolocationDto?> connectedClients = new();

    public void AddClient(string connectionId, TruckGeolocationDto? geolocationData)
    {
        connectedClients.TryAdd(connectionId, geolocationData);
    }

    public TruckGeolocationDto? GetGeolocationData(string connectionId)
    {
        connectedClients.TryGetValue(connectionId, out var geolocationData);
        return geolocationData;
    }

    public void UpdateGeolocationData(string connectionId, TruckGeolocationDto truckGeolocationDto)
    {
        connectedClients.AddOrUpdate(connectionId, truckGeolocationDto, (_, _) => truckGeolocationDto);
    }

    public void RemoveClient(string connectionId)
    {
        connectedClients.TryRemove(connectionId, out _);
    }
}
