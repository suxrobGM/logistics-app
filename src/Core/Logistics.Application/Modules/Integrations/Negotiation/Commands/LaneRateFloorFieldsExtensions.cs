using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal static class LaneRateFloorFieldsExtensions
{
    public static LaneRateFloor ToEntity(this ILaneRateFloorFields fields)
    {
        var floor = new LaneRateFloor { MinRatePerMile = fields.MinRatePerMile };
        fields.ApplyTo(floor);
        return floor;
    }

    /// <summary>Normalizes the lane and copies every field onto <paramref name="floor"/>.</summary>
    public static void ApplyTo(this ILaneRateFloorFields fields, LaneRateFloor floor)
    {
        floor.OriginCountry = LaneKey.Country(fields.OriginCountry);
        floor.OriginState = LaneKey.State(fields.OriginState);
        floor.DestinationCountry = LaneKey.Country(fields.DestinationCountry);
        floor.DestinationState = LaneKey.State(fields.DestinationState);
        floor.MinRatePerMile = fields.MinRatePerMile;
        floor.MinTotalRate = fields.MinTotalRateAmount.HasValue
            ? new Money { Amount = fields.MinTotalRateAmount.Value, Currency = fields.MinTotalRateCurrency }
            : null;
        floor.Notes = fields.Notes;
    }
}
