namespace Logistics.DriverApp.Services.LocationTracking;

public interface ILocationTracker : IAsyncDisposable
{
    Task<Location?> SendLocationDataAsync(LocationTrackerOptions options);
}
