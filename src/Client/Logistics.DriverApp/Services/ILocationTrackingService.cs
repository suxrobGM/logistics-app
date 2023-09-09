namespace Logistics.DriverApp.Services;

public interface ILocationTrackingService : IAsyncDisposable
{
    Task SendLocationDataAsync();
}
