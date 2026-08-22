using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal sealed class CreateLaneRateFloorHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateLaneRateFloorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateLaneRateFloorCommand req, CancellationToken ct)
    {
        var floor = req.ToEntity();

        var conflict = await LaneRateFloorUniqueness.FindConflictAsync(
            tenantUow, floor.OriginCountry, floor.OriginState,
            floor.DestinationCountry, floor.DestinationState, excludeId: null, ct);

        if (conflict is not null)
        {
            return Result<Guid>.Fail(conflict);
        }

        await tenantUow.Repository<LaneRateFloor>().AddAsync(floor, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<Guid>.Ok(floor.Id);
    }
}
