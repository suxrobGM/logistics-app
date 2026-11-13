using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.AI.Negotiation.Commands;

internal static class LaneRateFloorFieldsExtensions
{
    extension(ILaneRateFloorFields fields)
    {
        public LaneRateFloor ToEntity()
        {
            var floor = new LaneRateFloor { MinRatePerKm = fields.MinRatePerKm };
            fields.ApplyTo(floor);
            return floor;
        }

        /// <summary>Normalizes the lane and copies every field onto <paramref name="floor"/>.</summary>
        public void ApplyTo(LaneRateFloor floor)
        {
            floor.OriginCountry = LaneKey.Country(fields.OriginCountry);
            floor.OriginState = LaneKey.State(fields.OriginState);
            floor.DestinationCountry = LaneKey.Country(fields.DestinationCountry);
            floor.DestinationState = LaneKey.State(fields.DestinationState);
            floor.MinRatePerKm = fields.MinRatePerKm;
            floor.MinTotalRate = fields.MinTotalRate;
            floor.Notes = fields.Notes;
        }
    }

}
