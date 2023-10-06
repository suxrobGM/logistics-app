using Logistics.Models;

namespace Logistics.API.Hubs;

public interface ILiveTrackingHubClient
{
    Task ReceiveGeolocationData(TruckGeolocationDto truckGeolocation);
}
