using Logistics.Application.Abstractions;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetAIQuotaStatusHandler(
    IAIQuotaService quotaService,
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetAIQuotaStatusQuery, Result<AIQuotaStatusDto>>
{
    public async Task<Result<AIQuotaStatusDto>> Handle(GetAIQuotaStatusQuery request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var status = await quotaService.GetQuotaStatusAsync(tenant.Id, ct);

        return Result<AIQuotaStatusDto>.Ok(new AIQuotaStatusDto
        {
            UsagePercent = status.WeeklyQuota > 0
                ? (double)status.UsedThisWeek / status.WeeklyQuota
                : 0,
            IsOverQuota = status.IsOverQuota,
            PlanName = status.PlanName,
            ResetsAt = status.ResetsAt
        });
    }
}
