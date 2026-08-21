using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal sealed class UpdateLaneRateFloorHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateLaneRateFloorCommand, Result>
{
    public async Task<Result> Handle(UpdateLaneRateFloorCommand req, CancellationToken ct)
    {
        var floor = await tenantUow.Repository<LaneRateFloor>().GetByIdAsync(req.Id, ct);

        if (floor is null)
        {
            return Result.Fail($"Could not find a lane rate floor with ID '{req.Id}'");
        }

        var conflict = await LaneRateFloorUniqueness.FindConflictAsync(
            tenantUow, LaneKey.Country(req.OriginCountry), LaneKey.State(req.OriginState),
            LaneKey.Country(req.DestinationCountry), LaneKey.State(req.DestinationState), req.Id, ct);

        if (conflict is not null)
        {
            return Result.Fail(conflict);
        }

        req.ApplyTo(floor);
        floor.UpdatedAt = DateTime.UtcNow;

        tenantUow.Repository<LaneRateFloor>().Update(floor);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
