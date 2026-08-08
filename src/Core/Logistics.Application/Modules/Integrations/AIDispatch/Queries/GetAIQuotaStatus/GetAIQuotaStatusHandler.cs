using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.AICopilot.Queries;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

/// <summary>One handler for both surfaces' quota queries; only the feature gate differs per query.</summary>
internal sealed class GetAIQuotaStatusHandler(
    IAIQuotaService quotaService,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAIQuotaStatusQuery, Result<AIQuotaStatusDto>>,
        IAppRequestHandler<GetAICopilotQuotaStatusQuery, Result<AIQuotaStatusDto>>
{
    public Task<Result<AIQuotaStatusDto>> Handle(GetAIQuotaStatusQuery request, CancellationToken ct)
    {
        return GetStatusAsync(ct);
    }

    public Task<Result<AIQuotaStatusDto>> Handle(GetAICopilotQuotaStatusQuery request, CancellationToken ct)
    {
        return GetStatusAsync(ct);
    }

    private async Task<Result<AIQuotaStatusDto>> GetStatusAsync(CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var status = await quotaService.GetQuotaStatusAsync(tenant.Id, ct);

        return Result<AIQuotaStatusDto>.Ok(status.ToDto());
    }
}
