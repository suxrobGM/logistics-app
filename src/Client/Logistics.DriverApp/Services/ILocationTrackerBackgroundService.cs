namespace Logistics.DriverApp.Services;

public interface ILocationTrackerBackgroundService
{
    void Start(string truckId);
    void Stop();
}
