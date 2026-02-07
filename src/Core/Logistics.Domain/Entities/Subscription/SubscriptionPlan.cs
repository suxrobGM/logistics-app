using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class SubscriptionPlan : AuditableEntity, IMasterEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public PlanTier Tier { get; set; }

    /// <summary>
    /// Base monthly subscription fee
    /// </summary>
    public required Money Price { get; set; }

    /// <summary>
    /// Per-truck monthly charge
    /// </summary>
    public required Money PerTruckPrice { get; set; }

    public string? StripePriceId { get; set; }
    public string? StripeProductId { get; set; }
    public string? StripePerTruckPriceId { get; set; }

    /// <summary>
    /// Maximum number of trucks allowed on this plan. Null means unlimited.
    /// </summary>
    public int? MaxTrucks { get; set; }

    /// <summary>
    /// Annual billing discount percentage (e.g. 15 = 15% off)
    /// </summary>
    public decimal AnnualDiscountPercent { get; set; }

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
    public virtual List<PlanFeature> Features { get; set; } = [];
}
