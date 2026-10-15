using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

internal static class DatMapper
{
    /// <summary>
    /// A search match as a listing. DAT gives a city, state and coordinates but no street or ZIP,
    /// so those stay empty; a match carries no posting expiry, so neither does the listing.
    /// </summary>
    public static LoadBoardListingDto? ToListingDto(DatMatch match)
    {
        var id = match.PostingId ?? match.MatchId;
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var distance = match.TripLength?.DistanceMiles;
        var (total, perMile) = ToRates(match.Rate, distance);

        return new LoadBoardListingDto
        {
            ExternalListingId = id,
            ProviderType = LoadBoardProviderType.Dat,
            ProviderName = "DAT",
            OriginAddress = ToAddress(match.Origin?.Point),
            OriginLocation = ToGeoPoint(match.Origin?.Point),
            DestinationAddress = ToAddress(match.Destination?.Point),
            DestinationLocation = ToGeoPoint(match.Destination?.Point),
            TotalRate = total,
            RatePerMile = perMile,
            Currency = match.Rate?.BaseRate?.Currency ?? "USD",
            Distance = distance,
            Weight = match.Specs?.WeightInPounds,
            Length = match.Specs?.LengthInFeet,
            PickupDateStart = match.Availability?.Earliest,
            PickupDateEnd = match.Availability?.Latest,
            EquipmentType = DatEquipment.FromCode(match.Specs?.RequiredEquipmentType),
            BrokerName = match.Poster?.Company,
            BrokerPhone = ToPhone(match.Poster?.Contact?.Phone),
            BrokerEmail = match.Poster?.Contact?.Email,
            BrokerCreditScore = match.Credit?.Score,
            BrokerDaysToPay = match.Credit?.DaysToPay,
            Status = LoadBoardListingStatus.Available
        };
    }

    /// <summary>
    /// DAT posts either a trip total (FLAT) or a per-mile rate; the other is derived from the trip
    /// length when there is one.
    /// </summary>
    private static (decimal? Total, decimal? PerMile) ToRates(DatRate? rate, double? distance)
    {
        if (rate?.BaseRate?.Amount is not { } amount || amount <= 0)
        {
            return (null, null);
        }

        var miles = distance is > 0 ? (decimal)distance.Value : (decimal?)null;
        return rate.RateBasedOn?.ToUpperInvariant() == "PER_MILE"
            ? (miles is { } m ? Math.Round(amount * m, 2) : null, amount)
            : (amount, miles is { } n ? Math.Round(amount / n, 2) : null);
    }

    private static Address ToAddress(DatPoint? point) =>
        new LoadBoardPlace { City = point?.City, State = point?.State }.ToAddress();

    /// <summary>
    /// The listing requires a location; (0, 0) marks a match DAT sent without coordinates.
    /// </summary>
    private static GeoPoint ToGeoPoint(DatPoint? point) =>
        new(point?.Longitude ?? 0, point?.Latitude ?? 0);

    private static string? ToPhone(DatPhone? phone)
    {
        if (string.IsNullOrEmpty(phone?.Number))
        {
            return null;
        }

        return string.IsNullOrEmpty(phone.Extension) ? phone.Number : $"{phone.Number} x{phone.Extension}";
    }
}
