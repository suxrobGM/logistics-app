using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
/// Owns which weekly budget a tenant is metered against. Every tenant gets one - unmetered tenants
/// report no usage at all, on the admin report and the tenant's own usage panel alike.
/// </summary>
public static class AIWeeklyBudget
{
    /// <summary>The tier whose budget meters tenants with no subscription of their own.</summary>
    public const PlanTier FallbackTier = PlanTier.Enterprise;

    /// <summary>Used only when the catalogue has no <see cref="FallbackTier"/> plan.</summary>
    public const decimal DefaultUsd = 75m;

    /// <summary>
    /// The budget an unsubscribed tenant is metered against. Mirrored by the data fix in the
    /// <c>RequireWeeklyAIBudget</c> migration - keep the two in step.
    /// </summary>
    public static decimal FallbackFrom(IEnumerable<SubscriptionPlan> catalogue) =>
        catalogue.FirstOrDefault(p => p.Tier == FallbackTier)?.WeeklyAIBudgetUsd ?? DefaultUsd;
}
