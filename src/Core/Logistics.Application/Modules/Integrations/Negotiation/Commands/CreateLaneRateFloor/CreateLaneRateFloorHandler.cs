using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal sealed class CreateLaneRateFloorHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateLaneRateFloorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateLaneRateFloorCommand req, CancellationToken ct)
    {
        var originCountry = req.OriginCountry.Trim().ToUpperInvariant();
        var originState = NormalizeState(req.OriginState);
        var destinationCountry = req.DestinationCountry.Trim().ToUpperInvariant();
        var destinationState = NormalizeState(req.DestinationState);

        var conflict = await LaneRateFloorUniqueness.FindConflictAsync(
            tenantUow, originCountry, originState, destinationCountry, destinationState, excludeId: null, ct);

        if (conflict is not null)
        {
            return Result<Guid>.Fail(conflict);
        }

        var floor = new LaneRateFloor
        {
            OriginCountry = originCountry,
            OriginState = originState,
            DestinationCountry = destinationCountry,
            DestinationState = destinationState,
            MinRatePerMile = req.MinRatePerMile,
            MinTotalRate = req.MinTotalRateAmount.HasValue
                ? new Money { Amount = req.MinTotalRateAmount.Value, Currency = req.MinTotalRateCurrency }
                : null,
            Notes = req.Notes
        };

        await tenantUow.Repository<LaneRateFloor>().AddAsync(floor, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<Guid>.Ok(floor.Id);
    }

    private static string? NormalizeState(string? state) =>
        string.IsNullOrWhiteSpace(state) ? null : state.Trim().ToUpperInvariant();
}
