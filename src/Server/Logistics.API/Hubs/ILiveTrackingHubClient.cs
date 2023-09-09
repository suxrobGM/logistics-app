namespace Logistics.API.Hubs;

public interface ILiveTrackingHubClient
{
    Task ReceiveGeolocationData(string userId, double latitude, double longitude);
}
