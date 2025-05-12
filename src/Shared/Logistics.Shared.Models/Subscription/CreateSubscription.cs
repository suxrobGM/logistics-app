using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public class CreateSubscription
{
    public SubscriptionStatus Status { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? PlanId { get; set; }
}
