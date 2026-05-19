using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Features;

namespace Logistics.Application.Modules.IdentityAccess.Features.Queries;

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
