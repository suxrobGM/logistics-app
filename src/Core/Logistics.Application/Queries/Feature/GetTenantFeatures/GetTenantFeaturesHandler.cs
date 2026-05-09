using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTenantFeaturesHandler(
    IFeatureService featureService,
    ICurrentTenantAccessor tenantAccessor)
    : IAppRequestHandler<GetTenantFeaturesQuery, Result<IReadOnlyList<FeatureStatusDto>>>
{
    public async Task<Result<IReadOnlyList<FeatureStatusDto>>> Handle(
        GetTenantFeaturesQuery req,
        CancellationToken ct)
    {
        var tenantId = req.TenantId ?? tenantAccessor.GetCurrentTenant().Id;
        var features = await featureService.GetAllFeatureStatusAsync(tenantId);
        return Result<IReadOnlyList<FeatureStatusDto>>.Ok(features);
    }
}
