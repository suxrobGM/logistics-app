using Logistics.Domain.Core;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class SubscriptionPlan : AuditableEntity, IMasterEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    /// <summary>
    /// Subscription price per employee
    /// </summary>
    public required Money Price { get; set; }
    public string? StripePriceId { get; set; }
    public string? StripeProductId { get; set; }
    
    /// <summary>
    /// Billing frequency
    /// </summary>
    public BillingInterval Interval { get; set; } = BillingInterval.Month;
    
    /// <summary>
    /// The number of intervals between subscription billings. For example, interval=month and interval_count=3 bills every 3 months
    /// </summary>
    public int IntervalCount { get; set; } = 1;
    public DateTime? BillingCycleAnchor { get; set; }
    public TrialPeriod TrialPeriod { get; set; } = TrialPeriod.None;
    
    public virtual List<Subscription> Subscriptions { get; set; } = [];
}
