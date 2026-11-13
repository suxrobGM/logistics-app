using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class PlanFeature : Entity, IMasterEntity
{
    public Guid PlanId { get; set; }
    public TenantFeature Feature { get; set; }
    public virtual SubscriptionPlan Plan { get; set; } = null!;
}
