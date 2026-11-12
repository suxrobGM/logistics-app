using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TruckMapper
{
    [MapperIgnoreSource(nameof(Truck.MainDriver))]
    [MapperIgnoreSource(nameof(Truck.SecondaryDriver))]
    [MapperIgnoreSource(nameof(Truck.CurrentTrailer))]
    [MapperIgnoreSource(nameof(Truck.Loads))]
    [MapperIgnoreSource(nameof(Truck.Trips))]
    [MapperIgnoreSource(nameof(Truck.Documents))]
    [MapperIgnoreTarget(nameof(TruckDto.MainDriver))]
    [MapperIgnoreTarget(nameof(TruckDto.SecondaryDriver))]
    [MapperIgnoreTarget(nameof(TruckDto.CurrentTrailerNumber))]
    [MapperIgnoreTarget(nameof(TruckDto.Loads))]
    private static partial TruckDto MapTruck(Truck entity);

    [MapProperty(nameof(AdrEquipment.AllowedClasses), nameof(AdrEquipmentDto.AllowedClasses), Use = nameof(ExpandClasses))]
    public static partial AdrEquipmentDto ToDto(this AdrEquipment entity);

    [MapProperty(nameof(AdrEquipmentDto.AllowedClasses), nameof(AdrEquipment.AllowedClasses), Use = nameof(CombineClasses))]
    public static partial AdrEquipment ToDomain(this AdrEquipmentDto dto);

    private static HazmatClass[] ExpandClasses(HazmatClassFlags flags) => flags.ToClasses();

    private static HazmatClassFlags CombineClasses(HazmatClass[] classes) => classes.ToFlags();

    extension(Truck entity)
    {
        /// <summary>
        ///     Maps a Truck entity to TruckDto without loads.
        /// </summary>
        public TruckDto ToDto()
        {
            var dto = MapTruck(entity);
            dto.MainDriver = entity.MainDriver?.ToDto();
            dto.SecondaryDriver = entity.SecondaryDriver?.ToDto();
            return dto;
        }

        /// <summary>
        ///     Maps a Truck entity to TruckDto with loads.
        /// </summary>
        public TruckDto ToDto(IEnumerable<LoadDto> loads)
        {
            var dto = entity.ToDto();
            dto.Loads = loads;
            return dto;
        }
    }
}
