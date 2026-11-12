using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TripStopMapper
{
    [MapperIgnoreSource(nameof(TripStop.DomainEvents))]
    [MapperIgnoreSource(nameof(TripStop.Trip))]
    [MapperIgnoreSource(nameof(TripStop.Load))]
    [MapperIgnoreSource(nameof(TripStop.DetentionAlertedAt))]
    [MapperIgnoreSource(nameof(TripStop.LateNotifiedAt))]
    public static partial TripStopDto ToDto(this TripStop entity);
}
