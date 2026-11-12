using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Operations.Trips.Services;

/// <summary>
/// Keeps the arrival estimates of a truck's active trip current from its live position.
/// </summary>
public interface IEtaService : IApplicationService
{
    /// <summary>
    /// Recomputes the ETAs of the truck's active trip, at most once per <see cref="Trip.EtaRefreshInterval"/>.
    /// Raises the late event for stops that run past their appointment. The caller owns SaveChangesAsync.
    /// </summary>
    /// <returns>The trip whose ETAs were recomputed, or null when nothing was due.</returns>
    Task<Trip?> RefreshAsync(Truck truck, DateTime now, CancellationToken ct = default);

    /// <summary>
    /// Drive time left on the trip: the last stop's ETA minus the dwell allowed at the stops before it.
    /// Null when the trip has no estimate yet.
    /// </summary>
    TimeSpan? GetRemainingDriveTime(Trip trip, DateTime now);
}
