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
    [MapProperty(nameof(Trip.Loads), nameof(TripDto.Loads))]
    public static partial TripDto ToDto(this Trip entity);
}
