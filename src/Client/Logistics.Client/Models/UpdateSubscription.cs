using Logistics.Shared.Consts;

namespace Logistics.Client.Models;

public class UpdateSubscription
{
    public string? Id { get; set; }
    public SubscriptionStatus? Status { get; set; }
    public string? TenantId { get; set; }
    public string? PlanId { get; set; }
}
