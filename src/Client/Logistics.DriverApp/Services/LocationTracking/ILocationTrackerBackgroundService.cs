namespace Logistics.DriverApp.Services.LocationTracking;

public interface ILocationTrackerBackgroundService
{
    void Start(LocationTrackerOptions options);
    void Stop();
}
