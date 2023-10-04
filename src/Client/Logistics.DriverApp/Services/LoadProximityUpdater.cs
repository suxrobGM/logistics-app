using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.Models;

namespace Logistics.DriverApp.Services;

public class LoadProximityUpdater : ILoadProximityUpdater
{
    private readonly IApiClient _apiClient;
    private LoadDto[]? _activeLoads;

    public LoadProximityUpdater(IApiClient apiClient)
    {
        _apiClient = apiClient;
        WeakReferenceMessenger.Default.Register<ActiveLoadsChangedMessage>(this, (_, m) => _activeLoads = m.Value);
    }

    public async Task UpdateLoadProximitiesAsync(Location currentLocation)
    {
        if (_activeLoads is null)
        {
            return;
        }
        
        foreach (var load in _activeLoads)
        {
            await UpdateLoadProximityAsync(load, currentLocation);
        }
    }

    private async Task UpdateLoadProximityAsync(LoadDto load, Location currentLocation)
    {
        var originDistance = Location.CalculateDistance(currentLocation, 
            load.OriginAddressLat!.Value,
            load.OriginAddressLong!.Value, 
            DistanceUnits.Kilometers);
        
        var destDistance = Location.CalculateDistance(currentLocation, 
            load.DestinationAddressLat!.Value,
            load.DestinationAddressLong!.Value, 
            DistanceUnits.Kilometers);

        if (originDistance <= 0.5)
        {
            await _apiClient.UpdateLoadProximity(new UpdateLoadProximity
            {
                LoadId = load.Id,
                CanConfirmPickUp = true
            });
        }
        
        if (destDistance <= 0.5)
        {
            await _apiClient.UpdateLoadProximity(new UpdateLoadProximity
            {
                LoadId = load.Id,
                CanConfirmDelivery = true
            });
        }
    }
}
