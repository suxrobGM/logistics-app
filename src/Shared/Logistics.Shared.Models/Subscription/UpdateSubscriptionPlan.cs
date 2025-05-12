using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record UpdateSubscriptionPlan
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public TrialPeriod? TrialPeriod { get; set; }
    public BillingInterval? Interval { get; set; }
    public int? IntervalCount { get; set; } 
}
