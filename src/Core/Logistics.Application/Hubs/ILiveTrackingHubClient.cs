using Logistics.Shared.Models;

namespace Logistics.Application.Hubs;

/// <summary>
/// Hub client for geolocation live tracking.
/// </summary>
public interface ILiveTrackingHubClient
{
    Task ReceiveGeolocationData(TruckGeolocationDto truckGeolocation);
}
