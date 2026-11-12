using Logistics.Domain.Entities;
using Logistics.Domain.Events;

namespace Logistics.Application.Modules.Operations.Trips.Services;

/// <summary>
/// Turns trip problems spotted elsewhere (ETA, ELD sync, detention) into AI trip alerts. Never saves
/// and never blocks the caller: the alert runs later as a background command, and only the trip's
/// alert conversation is added to the caller's unit of work.
/// </summary>
public interface ITripAlertReporter : IApplicationService
{
    /// <summary>
    /// Flags the driver's active trip, once, when they have fewer driving minutes left than the trip
    /// still needs. The caller owns SaveChangesAsync.
    /// </summary>
    Task CheckHosShortfallAsync(Guid employeeId, int drivingMinutesRemaining, CancellationToken ct = default);

    /// <summary>
    /// The same check from the other side: a trip whose ETAs just changed, against its driver's last
    /// known hours. The caller owns SaveChangesAsync.
    /// </summary>
    Task CheckHosShortfallAsync(Trip trip, CancellationToken ct = default);

    /// <summary>Alerts the driver's active trip about HOS violations the ELD just reported.</summary>
    Task ReportViolationsAsync(Guid employeeId, int newViolations, CancellationToken ct = default);

    Task ReportLateStopAsync(TripStopRunningLateEvent late, CancellationToken ct = default);

    Task ReportDetentionAsync(Guid tripId, string details, CancellationToken ct = default);
}
