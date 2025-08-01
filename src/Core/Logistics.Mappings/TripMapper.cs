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
    [MapPropertyFromSource(nameof(TripDto.OriginAddress), Use = nameof(MapOriginAddress))]
    [MapPropertyFromSource(nameof(TripDto.DestinationAddress), Use = nameof(MapDestinationAddress))]
    [MapPropertyFromSource(nameof(TripDto.Loads), Use = nameof(MapLoads))]
    public static partial TripDto ToDto(this Trip entity);
    
    private static IEnumerable<TripLoadDto> MapLoads(Trip trip) =>
        trip.GetLoads().Select(LoadMapper.ToTripLoadDto);

    private static Address MapOriginAddress(Trip trip) => trip.GetOriginAddress();
    private static Address MapDestinationAddress(Trip trip) => trip.GetDestinationAddress();
}
