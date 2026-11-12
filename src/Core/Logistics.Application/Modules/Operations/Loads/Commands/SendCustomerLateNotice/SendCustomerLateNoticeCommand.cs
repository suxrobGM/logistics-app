namespace Logistics.Application.Modules.Operations.Loads.Commands;

/// <summary>Carries the tenant because a background job has no request to resolve one from.</summary>
public record SendCustomerLateNoticeCommand : ICommand
{
    public required Guid TenantId { get; init; }
    public required Guid StopId { get; init; }
    public required DateTime EstimatedArrivalAt { get; init; }
}
