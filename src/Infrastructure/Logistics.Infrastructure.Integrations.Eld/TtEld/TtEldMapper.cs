using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.Eld.TtEld;

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
            ExternalVehicleId = data.Id ?? "",
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

    public static EldHosLogEntryDto MapToLogDto(TtEldTrackingPoint data, string vehicleId)
    {
        var timestamp = DateTime.TryParse(data.Date, out var ts) ? ts.ToUniversalTime() : DateTime.UtcNow;
        return new EldHosLogEntryDto
        {
            ExternalLogId = null,
            ExternalDriverId = data.DriverId ?? vehicleId,
            LogDate = timestamp.Date,
            DutyStatus = DutyStatus.Driving, // GPS tracking point - no HOS data available from TT ELD
            StartTime = timestamp,
            EndTime = null,
            DurationMinutes = 0,
            Location = data.Address,
            Latitude = data.Coordinates?.Lat,
            Longitude = data.Coordinates?.Lng,
            Remark = data.Speed.HasValue ? $"Speed: {data.Speed} | Odometer: {data.Odometer}" : null
        };
    }
}
