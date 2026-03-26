using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetAiQuotaStatusHandler(
    IAiQuotaService quotaService,
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetAiQuotaStatusQuery, Result<AiQuotaStatusDto>>
{
    public async Task<Result<AiQuotaStatusDto>> Handle(GetAiQuotaStatusQuery request, CancellationToken ct)
    {
        var tenantId = tenantUow.GetCurrentTenant().Id;
        var status = await quotaService.GetQuotaStatusAsync(tenantId, ct);

        return Result<AiQuotaStatusDto>.Ok(new AiQuotaStatusDto
        {
            WeeklyQuota = status.WeeklyQuota,
            UsedThisWeek = status.UsedThisWeek,
            Remaining = status.Remaining,
            IsOverQuota = status.IsOverQuota
        });
    }
}
