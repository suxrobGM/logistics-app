using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
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

        var originCountry = req.OriginCountry.Trim().ToUpperInvariant();
        var originState = NormalizeState(req.OriginState);
        var destinationCountry = req.DestinationCountry.Trim().ToUpperInvariant();
        var destinationState = NormalizeState(req.DestinationState);

        var conflict = await LaneRateFloorUniqueness.FindConflictAsync(
            tenantUow, originCountry, originState, destinationCountry, destinationState, req.Id, ct);

        if (conflict is not null)
        {
            return Result.Fail(conflict);
        }

        floor.OriginCountry = originCountry;
        floor.OriginState = originState;
        floor.DestinationCountry = destinationCountry;
        floor.DestinationState = destinationState;
        floor.MinRatePerMile = req.MinRatePerMile;
        floor.MinTotalRate = req.MinTotalRateAmount.HasValue
            ? new Money { Amount = req.MinTotalRateAmount.Value, Currency = req.MinTotalRateCurrency }
            : null;
        floor.Notes = req.Notes;
        floor.UpdatedAt = DateTime.UtcNow;

        tenantUow.Repository<LaneRateFloor>().Update(floor);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }

    private static string? NormalizeState(string? state) =>
        string.IsNullOrWhiteSpace(state) ? null : state.Trim().ToUpperInvariant();
}
