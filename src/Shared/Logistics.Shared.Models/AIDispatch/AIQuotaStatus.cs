namespace Logistics.Shared.Models;

/// <summary>
/// Raw weekly budget figures in USD of model cost. Projected onto <see cref="AIQuotaStatusDto"/>
/// before leaving the API - tenants see a percentage, never dollars.
/// </summary>
public record AIQuotaStatus(
    decimal WeeklyBudgetUsd,
    decimal SpentThisWeekUsd,
    bool IsOverQuota,
    string? PlanName = null,
    DateTime? ResetsAt = null);
