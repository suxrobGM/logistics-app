using Logistics.Shared.Enums;

namespace Logistics.Client.Models;

public class CreateSubscription
{
    public SubscriptionStatus Status { get; set; }
    public string? TenantId { get; set; }
    public string? PlanId { get; set; }
}
