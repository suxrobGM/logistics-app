namespace Logistics.DriverApp.Services;

public interface ILocationTracker : IAsyncDisposable
{
    Task SendLocationDataAsync();
}
