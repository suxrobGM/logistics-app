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
    public decimal AnnualDiscountPercent { get; set; }
    public string? StripePriceId { get; set; }
    public string? StripeProductId { get; set; }
    public TrialPeriod TrialPeriod { get; set; } = TrialPeriod.ThirtyDays;
    public BillingInterval Interval { get; set; } = BillingInterval.Month;
    public int IntervalCount { get; set; } = 1;
    public List<TenantFeature> Features { get; set; } = [];
}
