using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.Application.Abstractions.AiDispatch;

public interface IAiQuotaService
{
    Task<AiQuotaStatus> GetQuotaStatusAsync(Guid tenantId, CancellationToken ct = default);
}

public record AiQuotaStatus(
    int WeeklyQuota,
    int UsedThisWeek,
    int Remaining,
    bool IsOverQuota,
    string? PlanName = null,
    DateTime ResetsAt = default,
    LlmModelTier AllowedModelTier = LlmModelTier.Base);
