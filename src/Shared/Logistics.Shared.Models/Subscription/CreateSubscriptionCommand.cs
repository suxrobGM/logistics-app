using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class CreateSubscriptionCommand
{
    public SubscriptionStatus Status { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? PlanId { get; set; }
}
