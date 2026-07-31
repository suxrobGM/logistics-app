using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Queries;

internal sealed class GetAICopilotQuotaStatusHandler(
    IAIQuotaService quotaService,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAICopilotQuotaStatusQuery, Result<AIQuotaStatusDto>>
{
    public async Task<Result<AIQuotaStatusDto>> Handle(GetAICopilotQuotaStatusQuery request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var status = await quotaService.GetQuotaStatusAsync(tenant.Id, ct);

        return Result<AIQuotaStatusDto>.Ok(status.ToDto());
    }
}
