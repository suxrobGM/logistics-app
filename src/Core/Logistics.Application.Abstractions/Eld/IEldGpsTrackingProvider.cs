using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Eld;

namespace Logistics.Application.Abstractions.Eld;

/// <summary>
/// Interface for ELD providers that support real-time GPS vehicle tracking.
/// Providers implementing this interface will have vehicle locations synced
/// during the periodic ELD sync job and broadcast via SignalR.
/// </summary>
public interface IEldGpsTrackingProvider
{
    Task<IEnumerable<EldVehicleLocationDto>> GetAllVehicleLocationsAsync(CancellationToken ct = default);
}
