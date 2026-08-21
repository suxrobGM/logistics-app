using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Queries;

internal sealed class GetLaneRateFloorsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetLaneRateFloorsQuery, Result<List<LaneRateFloorDto>>>
{
    public async Task<Result<List<LaneRateFloorDto>>> Handle(GetLaneRateFloorsQuery req, CancellationToken ct)
    {
        var floors = await tenantUow.Repository<LaneRateFloor>().GetListAsync(ct: ct);
        var dtos = floors.Select(f => f.ToDto()).ToList();
        return Result<List<LaneRateFloorDto>>.Ok(dtos);
    }
}
