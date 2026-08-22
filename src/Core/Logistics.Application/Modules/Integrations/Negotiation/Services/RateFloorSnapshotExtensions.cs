using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

public static class RateFloorSnapshotExtensions
{
    /// <summary>
    /// Reduces a resolved floor to what a negotiation stores. The total recorded is the one the
    /// offer was actually checked against - the per-mile and flat components already combined for
    /// the listing's distance - so a booking checked against the snapshot meets the same bar the
    /// counter-offer did.
    /// </summary>
    public static RateFloorSnapshot ToSnapshot(this EffectiveRateFloorDto floor, string currency)
    {
        if (!floor.HasFloor)
        {
            return RateFloorSnapshot.None;
        }

        var total = floor.EffectiveFloorTotal is { } amount
            ? new Money { Amount = amount, Currency = floor.MinTotalRate?.Currency ?? currency }
            : null;

        return new RateFloorSnapshot(floor.MinRatePerMile, total, floor.Source);
    }

    /// <summary>
    /// The floor frozen on an open thread, in the shape the floor check reads. Every round after the
    /// first is checked against this rather than a fresh resolve, so an edit to the lane floor
    /// cannot move the bar under a negotiation that is already running.
    /// </summary>
    public static EffectiveRateFloorDto ToEffectiveFloor(this RateNegotiation negotiation) => new()
    {
        HasFloor = negotiation.FloorSource != RateFloorSource.None,
        MinRatePerMile = negotiation.FloorRatePerMile,
        MinTotalRate = negotiation.FloorTotalRate,
        Source = negotiation.FloorSource,
        EffectiveFloorTotal = negotiation.FloorTotalRate?.Amount
    };
}
