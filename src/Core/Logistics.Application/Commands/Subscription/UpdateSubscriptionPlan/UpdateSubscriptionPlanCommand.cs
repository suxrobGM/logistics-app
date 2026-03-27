using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class UpdateSubscriptionPlanCommand : IAppRequest
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public PlanTier? Tier { get; set; }
    public decimal? PerTruckPrice { get; set; }
    public int? MaxTrucks { get; set; }
    public int? WeeklyAiSessionQuota { get; set; }
    public BillingInterval? Interval { get; set; }
    public int? IntervalCount { get; set; }
}
