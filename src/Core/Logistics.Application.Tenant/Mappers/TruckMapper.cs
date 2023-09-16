using Logistics.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class TruckMapper
{
    public static TruckDto ToDto(this Truck entity)
    {
        return new TruckDto
        {
            Id = entity.Id,
            TruckNumber = entity.TruckNumber,
            CurrentLocation = entity.Drivers.FirstOrDefault()?.LastKnownLocation,
            Drivers = entity.Drivers.Select(i => i.ToDto())
        };
    }
}
