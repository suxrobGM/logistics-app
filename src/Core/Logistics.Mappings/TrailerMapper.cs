using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TrailerMapper
{
    [MapperIgnoreSource(nameof(Trailer.DomainEvents))]
    [MapperIgnoreSource(nameof(Trailer.CreatedBy))]
    [MapperIgnoreSource(nameof(Trailer.UpdatedAt))]
    [MapperIgnoreSource(nameof(Trailer.UpdatedBy))]
    [MapperIgnoreTarget(nameof(TrailerDto.CurrentTruckId))]
    [MapperIgnoreTarget(nameof(TrailerDto.CurrentTruckNumber))]
    public static partial TrailerDto ToDto(this Trailer entity);

    public static TrailerDto ToDto(this Trailer entity, Truck? currentTruck)
    {
        var dto = entity.ToDto();
        return dto with { CurrentTruckId = currentTruck?.Id, CurrentTruckNumber = currentTruck?.Number };
    }
}
