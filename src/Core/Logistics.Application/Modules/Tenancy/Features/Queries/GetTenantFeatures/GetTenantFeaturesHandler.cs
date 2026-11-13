using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Features.Queries;

internal sealed class GetTenantFeaturesHandler(
    IFeatureService featureService,
    ICurrentTenantAccessor tenantAccessor)
    : IRequestHandler<GetTenantFeaturesQuery, Result<IReadOnlyList<FeatureStatusDto>>>
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
