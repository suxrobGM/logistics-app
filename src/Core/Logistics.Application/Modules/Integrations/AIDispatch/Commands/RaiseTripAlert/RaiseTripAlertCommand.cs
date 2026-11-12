using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

/// <summary>Carries the tenant because a background job has no request to resolve one from.</summary>
public record RaiseTripAlertCommand : ICommand
{
    public required Guid TenantId { get; init; }
    public required Guid TripId { get; init; }
    public required TripAlertKind Kind { get; init; }

    public required string Details { get; init; }
}
