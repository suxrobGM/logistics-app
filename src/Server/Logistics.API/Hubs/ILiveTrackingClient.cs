namespace Logistics.API.Hubs;

public interface ILiveTrackingClient
{
    Task ReceiveGeolocationData(string userId, double latitude, double longitude);
}
