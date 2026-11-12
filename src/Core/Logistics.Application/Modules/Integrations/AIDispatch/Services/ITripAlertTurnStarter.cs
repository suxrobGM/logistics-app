using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

/// <summary>
/// Starts a dispatch agent turn about a problem on an active trip, with nobody prompting. The agent
/// can only propose: every write it suggests still waits for a dispatcher.
/// </summary>
public interface ITripAlertTurnStarter : IApplicationService
{
    /// <summary>
    /// Appends the alert to the trip's alert conversation, asks for a turn and tells the dispatchers.
    /// Does nothing once the tenant has used its daily alert allowance. Saves its own changes.
    /// </summary>
    Task RaiseAsync(Trip trip, TripAlertKind kind, string details, CancellationToken ct = default);

    /// <summary>Retry entry point for an alert that arrived while another turn was running.</summary>
    Task TryWakeAsync(Guid tripId, CancellationToken ct = default);
}
