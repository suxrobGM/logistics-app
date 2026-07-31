namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
/// Converts a session's raw model cost into Stripe metered billing units. Lives in Payments
/// because the unit price is a billing concern - the AI layer reports raw cost only.
/// </summary>
internal static class AIOverageBilling
{
    /// <summary>
    /// Dollars per metered unit. <see cref="StripePlanService"/> derives the Stripe price from
    /// this - changing it recreates the price on the next reconcile.
    /// </summary>
    public const decimal UnitUsd = 0.10m;

    /// <summary>
    /// Markup on raw model cost. Tunable; covers never-metered budget burn (failed/cancelled/
    /// copilot sessions), Stripe fees, and margin.
    /// </summary>
    public const decimal CostMarkup = 3m;

    /// <summary>Whole units, rounded up, minimum one.</summary>
    public static int UnitsFor(decimal sessionCostUsd) =>
        Math.Max(1, (int)decimal.Ceiling(sessionCostUsd * CostMarkup / UnitUsd));
}
