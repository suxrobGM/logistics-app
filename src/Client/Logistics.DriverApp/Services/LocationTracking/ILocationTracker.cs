namespace Logistics.DriverApp.Services.LocationTracking;

public interface ILocationTracker : IAsyncDisposable
{
    Task SendLocationDataAsync(LocationTrackerOptions options);
}
