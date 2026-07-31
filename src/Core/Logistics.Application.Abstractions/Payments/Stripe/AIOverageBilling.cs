using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.Payments.Stripe;

/// <summary>
/// Owns both halves of overage billing - which sessions bill and what they cost - for Stripe
/// metering and the tenant-visible accrued figure alike. Split them and the number a tenant reads
/// stops matching the invoice they get.
/// </summary>
public static class AIOverageBilling
{
    /// <summary>
    /// Dollars per metered unit. StripePlanService derives the Stripe price from this - changing
    /// it recreates the price on the next reconcile.
    /// </summary>
    public const decimal UnitUsd = 0.10m;

    /// <summary>
    /// Markup on raw model cost. Tunable; covers never-metered budget burn (failed and cancelled
    /// sessions, and the run that crosses the budget), Stripe fees, and margin.
    /// </summary>
    public const decimal CostMarkup = 3m;

    /// <summary>
    /// Whether overage can reach an invoice at all. Gates the <c>IsOverage</c> stamp as well as the
    /// meter call, so a tenant Stripe cannot bill is never shown charges.
    /// </summary>
    public static bool CanBill(Tenant tenant) =>
        tenant.Subscription is not null && !string.IsNullOrEmpty(tenant.StripeCustomerId);

    /// <summary>Whole units, rounded up, minimum one.</summary>
    public static int UnitsFor(decimal sessionCostUsd) =>
        Math.Max(1, (int)decimal.Ceiling(sessionCostUsd * CostMarkup / UnitUsd));

    /// <summary>
    /// Which sessions bill. A failed or cancelled run still spent its budget, but billing for a
    /// turn that produced no answer is not defensible - that gap is priced into
    /// <see cref="CostMarkup"/>. Query form, so EF can translate it.
    /// </summary>
    public static readonly Expression<Func<AgentSession, bool>> Billable =
        session => session.IsOverage && session.Status == AgentSessionStatus.Completed;

    private static readonly Func<AgentSession, bool> BillableFn = Billable.Compile();

    /// <summary><see cref="Billable"/> for a session already in hand.</summary>
    public static bool IsBillable(AgentSession session) => BillableFn(session);
}
