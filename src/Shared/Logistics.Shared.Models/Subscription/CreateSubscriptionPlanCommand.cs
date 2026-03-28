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
    public int? WeeklyAiRequestQuota { get; set; }
    public LlmModelTier AllowedModelTier { get; set; }
    public BillingInterval Interval { get; set; }
    public int IntervalCount { get; set; }
}
