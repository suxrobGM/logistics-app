namespace Logistics.Shared.Models;

/// <summary>
/// Raw weekly AI quota figures for one tenant, as computed by the quota service. Projected onto
/// <see cref="AIQuotaStatusDto"/> before it leaves the API - the tenant-facing shape shows a
/// percentage rather than the raw counts.
/// </summary>
public record AIQuotaStatus(
    int WeeklyQuota,
    int UsedThisWeek,
    int Remaining,
    bool IsOverQuota,
    string? PlanName = null,
    DateTime? ResetsAt = null);
