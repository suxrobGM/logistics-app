using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TripStopMapper
{
    [MapperIgnoreSource(nameof(TripStop.Trip))]
    [MapperIgnoreSource(nameof(TripStop.Load))]
    public static partial TripStopDto ToDto(this TripStop entity);
}
