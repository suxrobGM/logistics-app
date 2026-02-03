using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDefaultFeaturesHandler(IFeatureService featureService)
    : IAppRequestHandler<GetDefaultFeaturesQuery, Result<IReadOnlyList<DefaultFeatureStatusDto>>>
{
    public async Task<Result<IReadOnlyList<DefaultFeatureStatusDto>>> Handle(
        GetDefaultFeaturesQuery req,
        CancellationToken ct)
    {
        var features = await featureService.GetDefaultFeaturesAsync();
        return Result<IReadOnlyList<DefaultFeatureStatusDto>>.Ok(features);
    }
}
