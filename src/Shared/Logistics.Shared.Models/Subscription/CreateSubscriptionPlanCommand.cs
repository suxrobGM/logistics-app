using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class CreateSubscriptionPlanCommand
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public PlanTier Tier { get; set; }
    public decimal PerTruckPrice { get; set; }
    public int? MaxTrucks { get; set; }
    public decimal AnnualDiscountPercent { get; set; }
    public TrialPeriod TrialPeriod { get; set; }
    public BillingInterval Interval { get; set; }
    public int IntervalCount { get; set; }
}
