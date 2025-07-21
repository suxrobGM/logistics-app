using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TripMapper
{
    [MapperIgnoreSource(nameof(Trip.DomainEvents))]
    [MapperIgnoreSource(nameof(Trip.Stops))]
    [MapProperty(nameof(Trip.Truck.Number), nameof(TripDto.TruckNumber))]
    [MapPropertyFromSource(nameof(TripDto.Loads), Use = nameof(MapLoads))]
    public static partial TripDto ToDto(this Trip entity);
    
    private static IEnumerable<TripLoadDto> MapLoads(Trip trip) =>
        trip.GetLoads().Select(LoadMapper.ToTripLoadDto);
}
