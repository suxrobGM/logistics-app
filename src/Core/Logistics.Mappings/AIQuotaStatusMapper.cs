using Logistics.Shared.Models;

namespace Logistics.Mappings;

/// <summary>
/// Projects the quota status onto the wire DTO. Shared by the dispatch and copilot quota queries -
/// the two surfaces read the same quota, so the percentage formula must not be able to disagree.
/// </summary>
public static class AIQuotaStatusMapper
{
    public static AIQuotaStatusDto ToDto(this AIQuotaStatus status) => new()
    {
        UsagePercent = status.WeeklyQuota > 0
            ? (double)status.UsedThisWeek / status.WeeklyQuota
            : 0,
        IsOverQuota = status.IsOverQuota,
        PlanName = status.PlanName,
        ResetsAt = status.ResetsAt
    };
}
