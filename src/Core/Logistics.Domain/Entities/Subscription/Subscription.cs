using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class Subscription : Entity, IMasterEntity
{
    public SubscriptionStatus Status { get; set; }
    public required Guid TenantId { get; set; }
    public virtual required Tenant Tenant { get; set; }
    public required Guid PlanId { get; set; }
    public virtual required SubscriptionPlan Plan { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
}
