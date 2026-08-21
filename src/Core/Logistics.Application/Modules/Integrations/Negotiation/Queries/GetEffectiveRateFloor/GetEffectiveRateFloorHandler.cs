using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Queries;

internal sealed class GetEffectiveRateFloorHandler(
    ITenantUnitOfWork tenantUow,
    ILaneRateFloorResolver resolver) : IAppRequestHandler<GetEffectiveRateFloorQuery, Result<EffectiveRateFloorDto>>
{
    public async Task<Result<EffectiveRateFloorDto>> Handle(GetEffectiveRateFloorQuery req, CancellationToken ct)
    {
        var listing = await tenantUow.Repository<LoadBoardListing>().GetByIdAsync(req.ListingId, ct);

        if (listing is null)
        {
            return Result<EffectiveRateFloorDto>.Fail($"Could not find a load board listing with ID '{req.ListingId}'");
        }

        var floor = await resolver.ResolveAsync(listing, ct);
        return Result<EffectiveRateFloorDto>.Ok(floor);
    }
}
