using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Utilities;

namespace Logistics.Domain.Entities;

public class Subscription : Entity, IMasterEntity
{
    public SubscriptionStatus Status { get; set; }
    public required Guid TenantId { get; set; }
    public virtual required Tenant Tenant { get; set; }
    public required Guid PlanId { get; set; }
    public virtual required SubscriptionPlan Plan { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }

    //public virtual List<SubscriptionInvoice> Invoices { get; set; } = [];

    /// <summary>
    ///     Creates a new trial subscription for a tenant with the specified plan
    /// </summary>
    /// <param name="tenant">Tenant entity</param>
    /// <param name="plan">Subscription plan</param>
    /// <returns>A new subscription object with trial status</returns>
    public static Subscription CreateTrial(Tenant tenant, SubscriptionPlan plan)
    {
        return new Subscription
        {
            Status = SubscriptionStatus.Trialing,
            NextBillingDate = SubscriptionUtils.GetNextBillingDate(plan.Interval, plan.IntervalCount),
            TrialEndDate = SubscriptionUtils.GetTrialEndDate(plan.TrialPeriod),
            TenantId = tenant.Id,
            Tenant = tenant,
            PlanId = plan.Id,
            Plan = plan
        };
    }
}
