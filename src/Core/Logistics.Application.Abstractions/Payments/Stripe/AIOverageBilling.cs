namespace Logistics.Application.Abstractions.Payments.Stripe;

/// <summary>
/// Converts a session's raw model cost into Stripe metered billing units. Single source of
/// billed-unit math for Stripe metering and the tenant-visible accrued overage figure.
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

    /// <summary>Whole units, rounded up, minimum one.</summary>
    public static int UnitsFor(decimal sessionCostUsd) =>
        Math.Max(1, (int)decimal.Ceiling(sessionCostUsd * CostMarkup / UnitUsd));
}
