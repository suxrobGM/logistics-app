using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TripMapper
{
    [MapperIgnoreSource(nameof(Trip.DomainEvents))]
    [MapperIgnoreSource(nameof(Trip.Stops))]
    [MapProperty(nameof(Trip.Truck.Number), nameof(TripDto.TruckNumber))]
    [MapProperty(nameof(Trip.Stops), nameof(TripDto.Stops))]
    [MapProperty(nameof(Trip.CreatedAt), nameof(TripDto.CreatedAt))]
    [MapPropertyFromSource(nameof(TripDto.OriginAddress), Use = nameof(MapOriginAddress))]
    [MapPropertyFromSource(nameof(TripDto.DestinationAddress), Use = nameof(MapDestinationAddress))]
    [MapPropertyFromSource(nameof(TripDto.Loads), Use = nameof(MapLoads))]
    public static partial TripDto ToDto(this Trip entity);

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
}
