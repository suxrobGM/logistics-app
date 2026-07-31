namespace Logistics.Shared.Models;

/// <summary>
/// Raw weekly budget figures in USD of model cost. Projected onto <see cref="AIQuotaStatusDto"/>
/// before leaving the API - tenants see a percentage, never budget dollars; only the billed
/// <see cref="OverageChargesUsd"/> is deliberately tenant-visible.
/// </summary>
/// <param name="WeeklyBudgetUsd">Always set - unsubscribed tenants fall back per <c>AIWeeklyBudget</c>.</param>
public record AIQuotaStatus(
    decimal WeeklyBudgetUsd,
    decimal SpentThisWeekUsd,
    bool IsOverQuota)
{
    public string? PlanName { get; init; }
    public DateTime? ResetsAt { get; init; }

    /// <summary>Billed (marked-up) overage accrued this week - what Stripe will invoice.</summary>
    public decimal OverageChargesUsd { get; init; }

    /// <summary>
    /// Whether over-budget sessions can be invoiced (<c>AIOverageBilling.CanBill</c>). False for an
    /// unsubscribed tenant, which runs past the budget free rather than accruing unbillable charges.
    /// </summary>
    public bool OverageBillable { get; init; }

    /// <summary>True when the budget is spent and the tenant opted to pause AI over billing overage.</summary>
    public bool OverageBlocked { get; init; }
}
