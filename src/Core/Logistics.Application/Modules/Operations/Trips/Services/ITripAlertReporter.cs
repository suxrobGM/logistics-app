using Logistics.Domain.Entities;
using Logistics.Domain.Events;

namespace Logistics.Application.Modules.Operations.Trips.Services;

/// <summary>Never saves: alerts run later as background commands. The caller owns SaveChangesAsync.</summary>
public interface ITripAlertReporter : IApplicationService
{
    /// <summary>
    /// Flags each driver's active trip once, when the driver lacks the driving minutes it needs. Takes the
    /// whole sync at once so the trips load in one query.
    /// </summary>
    Task CheckHosShortfallsAsync(
        IReadOnlyDictionary<Guid, int> drivingMinutesByDriver, CancellationToken ct = default);

    /// <summary>The same check after an ETA refresh, against the driver's last known hours.</summary>
    Task CheckHosShortfallAsync(Trip trip, CancellationToken ct = default);

    Task ReportViolationsAsync(Guid employeeId, int newViolations, CancellationToken ct = default);

    Task ReportLateStopAsync(TripStopRunningLateEvent late, CancellationToken ct = default);

    Task ReportDetentionAsync(Guid tripId, string details, CancellationToken ct = default);
}
