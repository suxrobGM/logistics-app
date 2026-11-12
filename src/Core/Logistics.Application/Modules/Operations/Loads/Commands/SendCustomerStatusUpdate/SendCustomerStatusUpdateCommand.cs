using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

/// <summary>Carries the tenant because a background job has no request to resolve one from.</summary>
public record SendCustomerStatusUpdateCommand : ICommand
{
    public required Guid TenantId { get; init; }
    public required Guid LoadId { get; init; }
    public required LoadStatus NewStatus { get; init; }
}
