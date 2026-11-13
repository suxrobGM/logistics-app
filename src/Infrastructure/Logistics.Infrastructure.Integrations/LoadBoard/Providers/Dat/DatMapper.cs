using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

internal static class DatMapper
{
    /// <summary>Maps a DAT match to a listing; null when it has no id. DAT provides no street, ZIP or posting expiry, so those stay empty.</summary>
    public static LoadBoardListingDto? ToListingDto(DatMatch match)
    {
        var id = match.PostingId ?? match.MatchId;
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var miles = match.TripLength?.DistanceMiles;
        var (total, perMile) = ToRates(match.Rate, miles);

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
            RatePerKm = perMile / ImperialUnits.KilometersPerMile,
            Currency = match.Rate?.BaseRate?.Currency ?? "USD",
            Distance = miles * ImperialUnits.MetersPerMile,
            WeightKg = match.Specs?.WeightInPounds * ImperialUnits.KilogramsPerPound,
            LengthMeters = match.Specs?.LengthInFeet * ImperialUnits.MetersPerFoot,
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

    /// <summary>Total and per-mile rate of a DAT posting. DAT posts one of the two and the other is derived from the trip length; both null when no rate is posted.</summary>
    private static (decimal? Total, decimal? PerMile) ToRates(DatRate? rate, double? distance)
    {
        if (rate?.BaseRate?.Amount is not { } amount || amount <= 0)
        {
            return (null, null);
        }

        var miles = distance is > 0 ? distance.Value.ToDecimal() : (decimal?)null;
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
