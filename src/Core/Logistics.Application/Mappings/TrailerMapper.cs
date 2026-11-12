using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TrailerMapper
{
    [MapperIgnoreTarget(nameof(TrailerDto.CurrentTruckId))]
    [MapperIgnoreTarget(nameof(TrailerDto.CurrentTruckNumber))]
    public static partial TrailerDto ToDto(this Trailer entity);

    extension(Trailer entity)
    {
        public TrailerDto ToDto(Truck? currentTruck)
        {
            var dto = entity.ToDto();
            return dto with { CurrentTruckId = currentTruck?.Id, CurrentTruckNumber = currentTruck?.Number };
        }
    }
}
