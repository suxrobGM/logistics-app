using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Services;

public static class RateFloorSnapshotExtensions
{
    extension(EffectiveRateFloorDto floor)
    {
        /// <summary>Converts the floor to the snapshot stored on a negotiation, including the total the offer was checked against, so booking later checks the same amount.</summary>
        public RateFloorSnapshot ToSnapshot(string currency)
        {
            if (!floor.HasFloor)
            {
                return RateFloorSnapshot.None;
            }

            var total = floor.EffectiveFloorTotal is { } amount
                ? new Money { Amount = amount, Currency = currency }
                : null;

            return new RateFloorSnapshot(floor.MinRatePerKm, total, floor.Source);
        }
    }

    extension(RateNegotiation negotiation)
    {
        /// <summary>Rebuilds the floor from the snapshot stored on the negotiation. Later rounds and booking use it, so editing a lane floor does not change the floor of an open thread.</summary>
        public EffectiveRateFloorDto ToEffectiveFloor() => new()
        {
            HasFloor = negotiation.FloorSource != RateFloorSource.None,
            MinRatePerKm = negotiation.FloorRatePerKm,
            MinTotalRate = negotiation.FloorTotalRate?.Amount,
            Source = negotiation.FloorSource,
            EffectiveFloorTotal = negotiation.FloorTotalRate?.Amount
        };
    }
}
