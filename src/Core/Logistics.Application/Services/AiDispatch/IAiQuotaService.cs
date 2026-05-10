using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Services;

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
