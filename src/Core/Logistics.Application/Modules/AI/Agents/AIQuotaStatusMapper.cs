using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.AI.Agents;

/// <summary>
/// Projects the quota status onto the wire DTO. Shared by the dispatch and copilot quota queries -
/// the two surfaces read the same quota, so the percentage formula must not be able to disagree.
/// </summary>
[Mapper]
public static partial class AIQuotaStatusMapper
{
    [MapPropertyFromSource(nameof(AIQuotaStatusDto.UsagePercent), Use = nameof(CalculateUsagePercent))]
    public static partial AIQuotaStatusDto ToDto(this AIQuotaStatus status);

    private static double CalculateUsagePercent(AIQuotaStatus status) =>
        status.WeeklyBudgetUsd > 0
            ? (double)(status.SpentThisWeekUsd / status.WeeklyBudgetUsd)
            : 0;
}
