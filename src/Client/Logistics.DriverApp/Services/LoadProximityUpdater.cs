using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Models;
using Logistics.Models;

namespace Logistics.DriverApp.Services;

public class LoadProximityUpdater : ILoadProximityUpdater
{
    private readonly IApiClient _apiClient;

    public LoadProximityUpdater(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task UpdateLoadProximitiesAsync(Location currentLocation)
    {
        var activeLoadsMessage = WeakReferenceMessenger.Default.Send<ActiveLoadsRequestMessage>();
        
        foreach (var load in activeLoadsMessage.Response)
        {
            await UpdateLoadProximityAsync(load, currentLocation);
        }
    }

    private async Task UpdateLoadProximityAsync(ActiveLoad load, Location currentLocation)
    {
        var originDistance = Location.CalculateDistance(currentLocation, 
            load.OriginAddressLat!.Value,
            load.OriginAddressLong!.Value, 
            DistanceUnits.Kilometers);
        
        var destDistance = Location.CalculateDistance(currentLocation, 
            load.DestinationAddressLat!.Value,
            load.DestinationAddressLong!.Value, 
            DistanceUnits.Kilometers);

        if (originDistance < 0.5 && !load.CanConfirmPickUp && load.Status != LoadStatusDto.PickedUp)
        {
            load.CanConfirmPickUp = true;
            await _apiClient.UpdateLoadProximity(new UpdateLoadProximity
            {
                LoadId = load.Id,
                CanConfirmPickUp = true
            });
        }
        
        if (destDistance < 0.5 && !load.CanConfirmDelivery && load.Status != LoadStatusDto.Delivered)
        {
            load.CanConfirmDelivery = true;
            await _apiClient.UpdateLoadProximity(new UpdateLoadProximity
            {
                LoadId = load.Id,
                CanConfirmDelivery = true
            });
        }
    }
}
