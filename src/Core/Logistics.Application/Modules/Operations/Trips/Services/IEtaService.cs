using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Operations.Trips.Services;

public interface IEtaService : IApplicationService
{
    /// <summary>
    /// At most once per <see cref="Trip.EtaRefreshInterval"/>; returns null when nothing was due.
    /// The caller owns SaveChangesAsync.
    /// </summary>
    Task<Trip?> RefreshAsync(Truck truck, DateTime now, CancellationToken ct = default);

    /// <summary>The last stop's ETA minus the dwell at the stops before it; null with no estimate yet.</summary>
    TimeSpan? GetRemainingDriveTime(Trip trip, DateTime now);
}
