using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.Eld.Samsara;

internal static class SamsaraMapper
{
    public static EldDriverHosDataDto MapToDto(string driverId, SamsaraHosClockData data)
    {
        return new EldDriverHosDataDto
        {
            ExternalDriverId = driverId,
            ExternalDriverName = data.Driver?.Name,
            CurrentDutyStatus = MapDutyStatus(data.CurrentDutyStatus),
            StatusChangedAt = DateTimeOffset.FromUnixTimeMilliseconds(data.CurrentDutyStatusStartMs ?? 0).UtcDateTime,
            DrivingMinutesRemaining = (int)(data.DrivingTimeRemainingMs ?? 0) / 60000,
            OnDutyMinutesRemaining = (int)(data.ShiftTimeRemainingMs ?? 0) / 60000,
            CycleMinutesRemaining = (int)(data.CycleTimeRemainingMs ?? 0) / 60000,
            TimeUntilBreakRequired = data.BreakTimeRemainingMs.HasValue
                ? TimeSpan.FromMilliseconds(data.BreakTimeRemainingMs.Value)
                : null,
            IsInViolation = data.CurrentViolations?.Count > 0
        };
    }

    public static EldHosLogEntryDto MapToLogDto(SamsaraHosLogData data)
    {
        return new EldHosLogEntryDto
        {
            ExternalLogId = data.Id,
            ExternalDriverId = data.Driver?.Id ?? "",
            LogDate = DateTimeOffset.FromUnixTimeMilliseconds(data.StartMs ?? 0).UtcDateTime.Date,
            DutyStatus = MapDutyStatus(data.DutyStatus),
            StartTime = DateTimeOffset.FromUnixTimeMilliseconds(data.StartMs ?? 0).UtcDateTime,
            EndTime = data.EndMs.HasValue
                ? DateTimeOffset.FromUnixTimeMilliseconds(data.EndMs.Value).UtcDateTime
                : null,
            DurationMinutes = (int)((data.EndMs ?? data.StartMs ?? 0) - (data.StartMs ?? 0)) / 60000,
            Location = data.Location?.Name,
            Latitude = data.Location?.Latitude,
            Longitude = data.Location?.Longitude,
            Remark = data.Remark
        };
    }

    public static EldViolationDataDto MapToViolationDto(string driverId, SamsaraViolationData data)
    {
        return new EldViolationDataDto
        {
            ExternalViolationId = data.Id,
            ExternalDriverId = driverId,
            ViolationDate = DateTimeOffset.FromUnixTimeMilliseconds(data.StartMs ?? 0).UtcDateTime,
            ViolationType = MapViolationType(data.ViolationType),
            Description = data.ViolationType ?? "Unknown violation",
            SeverityLevel = 3
        };
    }

    public static EldDriverDto MapToDriverDto(SamsaraDriverData data)
    {
        return new EldDriverDto
        {
            ExternalDriverId = data.Id ?? "",
            Name = data.Name ?? "",
            Email = data.Email,
            Phone = data.Phone,
            LicenseNumber = data.LicenseNumber
        };
    }

    public static EldVehicleDto MapToVehicleDto(SamsaraVehicleData data)
    {
        return new EldVehicleDto
        {
            ExternalVehicleId = data.Id ?? "",
            Name = data.Name ?? "",
            Vin = data.Vin,
            LicensePlate = data.LicensePlate,
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
            "sleeper_berth" or "sleeper" => DutyStatus.SleeperBerth,
            "driving" => DutyStatus.Driving,
            "on_duty" or "on_duty_not_driving" => DutyStatus.OnDutyNotDriving,
            "yard_move" => DutyStatus.YardMove,
            "personal_conveyance" => DutyStatus.PersonalConveyance,
            _ => DutyStatus.OffDuty
        };
    }

    public static HosViolationType MapViolationType(string? type)
    {
        return type?.ToLowerInvariant() switch
        {
            "driving" or "11_hour" => HosViolationType.Driving11Hour,
            "shift" or "14_hour" => HosViolationType.OnDuty14Hour,
            "break" or "30_minute" => HosViolationType.Break30Minute,
            "cycle" or "70_hour" => HosViolationType.Cycle70Hour,
            "restart" => HosViolationType.RestartRequired,
            _ => HosViolationType.FormAndMannerViolation
        };
    }
}
