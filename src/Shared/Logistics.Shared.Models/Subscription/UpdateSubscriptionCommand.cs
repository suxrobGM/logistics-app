using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class UpdateSubscriptionCommand
{
    public string? Id { get; set; }
    public SubscriptionStatus? Status { get; set; }
    public string? TenantId { get; set; }
    public string? PlanId { get; set; }
}
