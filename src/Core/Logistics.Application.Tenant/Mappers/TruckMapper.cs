using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class TruckMapper
{
    public static TruckDto ToDto(this Truck entity, IEnumerable<LoadDto> loads)
    {
        var dto = new TruckDto
        {
            Id = entity.Id,
            TruckNumber = entity.TruckNumber,
            CurrentLocationLat = entity.CurrentLocationLat,
            CurrentLocationLong = entity.CurrentLocationLong,
            Loads = loads,
            Drivers = entity.Drivers.Select(i => i.ToDto())
        };

        if (entity.CurrentLocation.IsNotNull())
        {
            dto.CurrentLocation = entity.CurrentLocation.ToDto();
        }
        return dto;
    }
}
