using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TruckMapper
{
    /// <summary>
    ///     Maps a Truck entity to TruckDto without loads.
    /// </summary>
    public static TruckDto ToDto(this Truck entity)
    {
        return new TruckDto
        {
            Id = entity.Id,
            Number = entity.Number,
            Type = entity.Type,
            Status = entity.Status,
            CurrentLocation = entity.CurrentLocation,
            CurrentAddress = entity.CurrentAddress,
            VehicleCapacity = entity.VehicleCapacity,
            Loads = [],
            MainDriver = entity.MainDriver?.ToDto(),
            SecondaryDriver = entity.SecondaryDriver?.ToDto(),
        };
    }

    /// <summary>
    ///     Maps a Truck entity to TruckDto with loads.
    /// </summary>
    public static TruckDto ToDto(this Truck entity, IEnumerable<LoadDto> loads)
    {
        var dto = entity.ToDto();
        dto.Loads = loads;
        return dto;
    }
}
