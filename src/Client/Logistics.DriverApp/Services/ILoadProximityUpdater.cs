namespace Logistics.DriverApp.Services;

public interface ILoadProximityUpdater
{
    public Task UpdateLoadProximitiesAsync(Location currentLocation);
}
