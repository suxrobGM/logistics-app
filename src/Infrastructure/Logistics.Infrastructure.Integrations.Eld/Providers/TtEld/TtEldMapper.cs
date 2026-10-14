using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;

internal static class TtEldMapper
{
    public static EldDriverDto MapToDriverDto(TtEldDriverData data)
    {
        return new EldDriverDto
        {
            ExternalDriverId = data.Id ?? "",
            Name = $"{data.FirstName} {data.SecondName}".Trim(),
            Email = null,
            Phone = null,
            LicenseNumber = null
        };
    }

    public static EldVehicleDto MapToVehicleDto(TtEldUnitData data)
    {
        return new EldVehicleDto
        {
            // Same key the location feed uses (it has no unit id), so a vehicle mapping matches its GPS pings
            ExternalVehicleId = data.Vin ?? data.TruckNumber ?? data.Id ?? "",
            Name = data.TruckNumber ?? "",
            Vin = data.Vin,
            LicensePlate = null,
            Make = null,
            Model = null,
            Year = null
        };
    }

    public static EldVehicleLocationDto MapToLocationDto(TtEldTrackingUnit data)
    {
        return new EldVehicleLocationDto
        {
            ExternalVehicleId = data.Vin ?? data.TruckNumber ?? "",
            TruckNumber = data.TruckNumber,
            Vin = data.Vin,
            Latitude = data.Coordinates?.Lat ?? 0,
            Longitude = data.Coordinates?.Lng ?? 0,
            Timestamp = DateTime.TryParse(data.Timestamp, out var ts) ? ts.ToUniversalTime() : DateTime.UtcNow
        };
    }
}
