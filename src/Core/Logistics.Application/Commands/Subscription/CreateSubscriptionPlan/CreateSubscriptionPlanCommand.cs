using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class CreateSubscriptionPlanCommand : IAppRequest
{
    public string Name { get; set; } = null!;
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
