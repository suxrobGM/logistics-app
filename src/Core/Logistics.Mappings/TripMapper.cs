using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
[UseStaticMapper(typeof(TripStopMapper))]
public static partial class TripMapper
{
    [MapperIgnoreSource(nameof(Trip.DomainEvents))]
    [MapProperty(nameof(Trip.Truck.Number), nameof(TripDto.TruckNumber))]
    [MapperIgnoreSource(nameof(Trip.Trailer))]
    [MapperIgnoreSource(nameof(Trip.DriverId))]
    [MapperIgnoreSource(nameof(Trip.SecondaryDriverId))]
    [MapperIgnoreTarget(nameof(TripDto.TrailerNumber))]
    [MapProperty(nameof(Trip.Stops), nameof(TripDto.Stops))]
    [MapProperty(nameof(Trip.CreatedAt), nameof(TripDto.CreatedAt))]
    [MapPropertyFromSource(nameof(TripDto.OriginAddress), Use = nameof(MapOriginAddress))]
    [MapPropertyFromSource(nameof(TripDto.DestinationAddress), Use = nameof(MapDestinationAddress))]
    [MapPropertyFromSource(nameof(TripDto.Loads), Use = nameof(MapLoads))]
    [MapPropertyFromSource(nameof(TripDto.LoadsCount), Use = nameof(MapLoadsCount))]
    [MapPropertyFromSource(nameof(TripDto.TotalRevenue), Use = nameof(MapTotalRevenue))]
    private static partial TripDto MapTrip(Trip entity);

    /// <summary>
    /// <paramref name="trailerNumber"/> is passed in rather than read off <c>Trip.Trailer</c>, which would lazy-load one
    /// trailer per row in a list.
    /// </summary>
    public static TripDto ToDto(this Trip entity, string? trailerNumber) =>
        MapTrip(entity) with { TrailerNumber = trailerNumber };

    private static IEnumerable<TripLoadDto> MapLoads(Trip trip)
    {
        return trip.GetLoads().Select(LoadMapper.ToTripLoadDto);
    }

    private static Address MapOriginAddress(Trip trip)
    {
        return trip.GetOriginAddress();
    }

    private static Address MapDestinationAddress(Trip trip)
    {
        return trip.GetDestinationAddress();
    }

    private static int MapLoadsCount(Trip trip)
    {
        return trip.GetLoads().Count();
    }

    private static decimal MapTotalRevenue(Trip trip)
    {
        return trip.CalcTotalRevenue();
    }
}
