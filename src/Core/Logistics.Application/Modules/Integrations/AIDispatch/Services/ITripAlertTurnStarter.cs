using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

public interface ITripAlertTurnStarter : IApplicationService
{
    /// <summary>Saves its own changes, and does nothing once the daily alert allowance is used.</summary>
    Task RaiseAsync(Trip trip, TripAlertKind kind, string details, CancellationToken ct = default);

    /// <summary>Retries an alert that arrived while another turn was running.</summary>
    Task TryWakeAsync(Guid tripId, CancellationToken ct = default);
}
