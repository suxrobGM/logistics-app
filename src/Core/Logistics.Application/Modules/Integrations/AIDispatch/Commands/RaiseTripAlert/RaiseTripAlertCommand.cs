using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

/// <summary>
/// Puts a problem on an active trip in front of the dispatch agent. Enqueued by the trigger that
/// spotted it, so it runs after that save in its own scope; carries the tenant because a background
/// job has no request to resolve one from.
/// </summary>
public record RaiseTripAlertCommand : ICommand
{
    public required Guid TenantId { get; init; }
    public required Guid TripId { get; init; }
    public required TripAlertKind Kind { get; init; }

    /// <summary>System-written facts: what happened, which stop, the numbers.</summary>
    public required string Details { get; init; }
}
