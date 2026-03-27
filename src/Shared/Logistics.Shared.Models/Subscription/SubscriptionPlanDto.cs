using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record SubscriptionPlanDto
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public PlanTier Tier { get; set; }
    public decimal Price { get; set; }
    public decimal PerTruckPrice { get; set; }
    public int? MaxTrucks { get; set; }
    public BillingInterval Interval { get; set; } = BillingInterval.Month;
    public int IntervalCount { get; set; } = 1;
    public int? WeeklyAiSessionQuota { get; set; }
    public List<TenantFeature> Features { get; set; } = [];
}
