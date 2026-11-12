using Logistics.Shared.Models;

namespace Logistics.Mappings;

/// <summary>
/// Projects the quota status onto the wire DTO. Shared by the dispatch and copilot quota queries -
/// the two surfaces read the same quota, so the percentage formula must not be able to disagree.
/// </summary>
public static class AIQuotaStatusMapper
{
    extension(AIQuotaStatus status)
    {
        public AIQuotaStatusDto ToDto() => new()
        {
            UsagePercent = status.WeeklyBudgetUsd > 0
                ? (double)(status.SpentThisWeekUsd / status.WeeklyBudgetUsd)
                : 0,
            IsOverQuota = status.IsOverQuota,
            PlanName = status.PlanName,
            ResetsAt = status.ResetsAt,
            OverageChargesUsd = status.OverageChargesUsd,
            OverageBlocked = status.OverageBlocked
        };
    }
}
