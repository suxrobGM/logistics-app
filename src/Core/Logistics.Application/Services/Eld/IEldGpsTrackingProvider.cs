using Logistics.Shared.Models;

namespace Logistics.Application.Services;

/// <summary>
/// Interface for ELD providers that support real-time GPS vehicle tracking.
/// Providers implementing this interface will have vehicle locations synced
/// during the periodic ELD sync job and broadcast via SignalR.
/// </summary>
public interface IEldGpsTrackingProvider
{
    Task<IEnumerable<EldVehicleLocationDto>> GetAllVehicleLocationsAsync(CancellationToken ct = default);
}
