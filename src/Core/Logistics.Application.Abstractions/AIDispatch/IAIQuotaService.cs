namespace Logistics.Application.Abstractions.AIDispatch;

public interface IAIQuotaService
{
    Task<AIQuotaStatus> GetQuotaStatusAsync(Guid tenantId, CancellationToken ct = default);
}

public record AIQuotaStatus(
    int WeeklyQuota,
    int UsedThisWeek,
    int Remaining,
    bool IsOverQuota,
    string? PlanName = null,
    DateTime? ResetsAt = null);
