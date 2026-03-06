using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.Eld.Motive;

internal static class MotiveMapper
{
    public static EldDriverHosDataDto MapToDto(MotiveHosData data)
    {
        return new EldDriverHosDataDto
        {
            ExternalDriverId = data.Driver?.Id?.ToString() ?? "",
            ExternalDriverName = $"{data.Driver?.FirstName} {data.Driver?.LastName}".Trim(),
            CurrentDutyStatus = MapDutyStatus(data.CurrentDutyStatus),
            StatusChangedAt = data.CurrentDutyStatusStartTime ?? DateTime.UtcNow,
            DrivingMinutesRemaining = data.DrivingTimeRemaining ?? 0,
            OnDutyMinutesRemaining = data.ShiftTimeRemaining ?? 0,
            CycleMinutesRemaining = data.CycleTimeRemaining ?? 0,
            TimeUntilBreakRequired = data.BreakTimeRemaining.HasValue
                ? TimeSpan.FromMinutes(data.BreakTimeRemaining.Value)
                : null,
            IsInViolation = data.ViolationCount > 0
        };
    }

    public static EldHosLogEntryDto MapToLogDto(string driverId, string? logDate, MotiveLogEvent data)
    {
        var date = DateTime.TryParse(logDate, out var d) ? d : DateTime.UtcNow.Date;
        return new EldHosLogEntryDto
        {
            ExternalLogId = data.Id?.ToString(),
            ExternalDriverId = driverId,
            LogDate = date,
            DutyStatus = MapDutyStatus(data.Type),
            StartTime = data.StartTime ?? date,
            EndTime = data.EndTime,
            DurationMinutes = data.Duration ?? 0,
            Location = data.Location,
            Latitude = data.Lat,
            Longitude = data.Lon,
            Remark = data.Annotation
        };
    }

    public static EldViolationDataDto MapToViolationDto(string driverId, MotiveViolationData data)
    {
        return new EldViolationDataDto
        {
            ExternalViolationId = data.Id?.ToString(),
            ExternalDriverId = driverId,
            ViolationDate = data.StartTime ?? DateTime.UtcNow,
            ViolationType = MapViolationType(data.Type),
            Description = data.Type ?? "Unknown violation",
            SeverityLevel = 3
        };
    }

    public static EldDriverDto MapToDriverDto(MotiveUserData data)
    {
        return new EldDriverDto
        {
            ExternalDriverId = data.Id?.ToString() ?? "",
            Name = $"{data.FirstName} {data.LastName}".Trim(),
            Email = data.Email,
            Phone = data.Phone,
            LicenseNumber = data.DriverLicenseNumber
        };
    }

    public static EldVehicleDto MapToVehicleDto(MotiveVehicleData data)
    {
        return new EldVehicleDto
        {
            ExternalVehicleId = data.Id?.ToString() ?? "",
            Name = data.Number ?? "",
            Vin = data.Vin,
            LicensePlate = data.LicensePlateNumber,
            Make = data.Make,
            Model = data.Model,
            Year = data.Year
        };
    }

    public static DutyStatus MapDutyStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "off_duty" or "off" => DutyStatus.OffDuty,
            "sleeper" or "sleeper_berth" => DutyStatus.SleeperBerth,
            "driving" or "d" => DutyStatus.Driving,
            "on_duty" or "on" => DutyStatus.OnDutyNotDriving,
            "yard_move" or "ym" => DutyStatus.YardMove,
            "personal_conveyance" or "pc" => DutyStatus.PersonalConveyance,
            _ => DutyStatus.OffDuty
        };
    }

    public static HosViolationType MapViolationType(string? type)
    {
        return type?.ToLowerInvariant() switch
        {
            "driving" or "11_hour_driving" => HosViolationType.Driving11Hour,
            "shift" or "14_hour_shift" => HosViolationType.OnDuty14Hour,
            "break" or "30_minute_break" => HosViolationType.Break30Minute,
            "cycle" or "60_70_hour" => HosViolationType.Cycle70Hour,
            "restart" => HosViolationType.RestartRequired,
            _ => HosViolationType.FormAndMannerViolation
        };
    }
}
