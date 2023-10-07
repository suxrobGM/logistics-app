using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class TruckMapper
{
    public static TruckDto ToDto(this Truck entity, IEnumerable<LoadDto> loads)
    {
        return new TruckDto
        {
            Id = entity.Id,
            TruckNumber = entity.TruckNumber,
            CurrentLocation = entity.LastKnownLocation,
            CurrentLocationLat = entity.LastKnownLocationLat,
            CurrentLocationLong = entity.LastKnownLocationLong,
            DriverIncomePercentage = entity.DriverIncomePercentage,
            Loads = loads,
            Drivers = entity.Drivers.Select(i => i.ToDto())
        };
    }
}
