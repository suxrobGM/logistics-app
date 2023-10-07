using Logistics.Shared.Models;

namespace Logistics.API.Hubs;

public interface ILiveTrackingHubClient
{
    Task ReceiveGeolocationData(TruckGeolocationDto truckGeolocation);
}
