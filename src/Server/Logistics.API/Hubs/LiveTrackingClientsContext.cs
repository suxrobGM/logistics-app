using Logistics.Models;

namespace Logistics.API.Hubs;

public class LiveTrackingClientsContext
{
    private readonly Dictionary<string, GeolocationData?> _connectedClients = new();

    public void AddClient(string connectionId, GeolocationData? geolocationData)
    {
        _connectedClients.Add(connectionId, geolocationData);
    }

    public GeolocationData? GetGeolocationData(string connectionId)
    {
        return _connectedClients.TryGetValue(connectionId, out var geolocationData) ? geolocationData : null;
    }

    public void UpdateData(string connectionId, GeolocationData geolocationData)
    {
        if (_connectedClients.ContainsKey(connectionId))
        {
            _connectedClients[connectionId] = geolocationData;
        }
    }
    
    public void RemoveClient(string connectionId)
    {
        _connectedClients.Remove(connectionId);
    }
}
