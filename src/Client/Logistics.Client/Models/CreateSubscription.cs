using Logistics.Shared.Consts;

namespace Logistics.Client.Models;

public class CreateSubscription
{
    public SubscriptionStatus Status { get; set; }
    public string? TenantId { get; set; }
    public string? PlanId { get; set; }
}
