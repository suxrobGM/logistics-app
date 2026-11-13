using Logistics.Application.Abstractions.Integrations;
using Logistics.Domain.Primitives;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

internal static class DatSearchRequestBuilder
{
    public static DatSearchRequest Build(LoadBoardSearchCriteria criteria, int maxAgeMinutes)
    {
        if (string.IsNullOrWhiteSpace(criteria.Origin?.City) || string.IsNullOrWhiteSpace(criteria.Origin?.State))
        {
            throw new LoadBoardApiException("DAT search needs an origin city and state.");
        }

        var equipment = criteria.EquipmentTypes is { Length: > 0 } types
            ? DatEquipment.ToSearchCodes(types).ToArray()
            : null;

        return new DatSearchRequest(new DatSearchCriteria
        {
            DesiredSpecs = new DatDesiredSpecs { EquipmentTypes = equipment },
            MaxAgeInMinutes = maxAgeMinutes,
            Availability = criteria.PickupDateStart is null && criteria.PickupDateEnd is null
                ? null
                : new DatAvailability
                {
                    Earliest = criteria.PickupDateStart?.ToUniversalTime(),
                    Latest = criteria.PickupDateEnd?.ToUniversalTime()
                },
            Origin = ToPlace(criteria.Origin, criteria.OriginRadiusMeters),
            // A destination point needs a city; a state-only destination is filtered locally.
            Destination = string.IsNullOrWhiteSpace(criteria.Destination?.City) || string.IsNullOrWhiteSpace(criteria.Destination.State)
                ? null
                : ToPlace(criteria.Destination, criteria.DestinationRadiusMeters)
        });
    }

    /// <summary>Applies the weight and length filters DAT's query cannot express; listings that post no weight or length pass.</summary>
    public static bool MatchesLocalFilters(LoadBoardListingDto listing, LoadBoardSearchCriteria criteria)
    {
        if (criteria.MaxWeightKg is { } maxWeight && listing.WeightKg > maxWeight)
        {
            return false;
        }

        if (criteria.MaxLengthMeters is { } maxLength && listing.LengthMeters > maxLength)
        {
            return false;
        }

        var destinationState = criteria.Destination?.State;
        return string.IsNullOrWhiteSpace(destinationState) ||
               !string.IsNullOrWhiteSpace(criteria.Destination?.City) ||
               string.Equals(listing.DestinationAddress.State, destinationState.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Converts a place to a DAT search point. DAT takes the deadhead radius in whole miles, so <paramref name="radiusMeters"/> is converted.</summary>
    private static DatPlace ToPlace(LoadBoardPlace place, int radiusMeters) => new(new DatPoint
    {
        City = place.City!.Trim(),
        State = place.State!.Trim().ToUpperInvariant(),
        Deadhead = new DatMiles { Miles = (int)Math.Round(radiusMeters / ImperialUnits.MetersPerMile) }
    });
}
