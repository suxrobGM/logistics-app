using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

/// <summary>
/// Turns our search criteria into a DAT freight-matching query, and applies the criteria DAT's
/// query has no field for to the matches it returns.
/// </summary>
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
            Origin = ToPlace(criteria.Origin, criteria.OriginRadius),
            // A destination point needs a city; a state-only destination is filtered locally.
            Destination = string.IsNullOrWhiteSpace(criteria.Destination?.City) || string.IsNullOrWhiteSpace(criteria.Destination.State)
                ? null
                : ToPlace(criteria.Destination, criteria.DestinationRadius)
        });
    }

    /// <summary>
    /// DAT's query takes no weight, length or state-only destination, so those are checked here.
    /// A listing that does not state its weight or length passes.
    /// </summary>
    public static bool MatchesLocalFilters(LoadBoardListingDto listing, LoadBoardSearchCriteria criteria)
    {
        if (criteria.MaxWeight is { } maxWeight && listing.Weight > maxWeight)
        {
            return false;
        }

        if (criteria.MaxLength is { } maxLength && listing.Length > maxLength)
        {
            return false;
        }

        var destinationState = criteria.Destination?.State;
        return string.IsNullOrWhiteSpace(destinationState) ||
               !string.IsNullOrWhiteSpace(criteria.Destination?.City) ||
               string.Equals(listing.DestinationAddress.State, destinationState.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static DatPlace ToPlace(LoadBoardPlace place, int radiusMiles) => new(new DatPoint
    {
        City = place.City!.Trim(),
        State = place.State!.Trim().ToUpperInvariant(),
        Deadhead = new DatMiles { Miles = radiusMiles }
    });
}
