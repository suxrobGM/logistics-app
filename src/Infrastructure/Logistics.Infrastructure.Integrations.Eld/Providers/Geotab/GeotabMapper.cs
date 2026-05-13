using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;

/// <summary>
/// Maps MyGeotab DutyStatusLog / DutyStatusViolation / Device / User entities to the
/// provider-agnostic ELD DTOs. Violation type mapping is region-aware: Geotab serves
/// both US ELD and EU tachograph fleets, so the same provider-string can mean
/// different things depending on the tenant's region.
/// </summary>
internal static class GeotabMapper
{
    public static EldDriverDto MapToDriverDto(GeotabUser user)
    {
        return new EldDriverDto
        {
            ExternalDriverId = user.Id ?? "",
            Name = user.Name ?? $"{user.FirstName} {user.LastName}".Trim(),
            Phone = user.PhoneNumber,
            LicenseNumber = user.LicenseNumber
        };
    }

    public static EldVehicleDto MapToVehicleDto(GeotabDevice device)
    {
        return new EldVehicleDto
        {
            ExternalVehicleId = device.Id ?? "",
            Name = device.Name ?? "",
            Vin = device.Vin,
            LicensePlate = device.LicensePlate
        };
    }

    public static EldHosLogEntryDto MapToLogDto(GeotabDutyStatusLog log)
    {
        var start = log.DateTime ?? DateTime.UtcNow;
        return new EldHosLogEntryDto
        {
            ExternalLogId = log.Id,
            ExternalDriverId = log.Driver?.Id ?? "",
            LogDate = start.Date,
            DutyStatus = MapDutyStatus(log.Status),
            StartTime = start,
            EndTime = null,
            DurationMinutes = 0,
            Location = log.Location?.Address,
            Latitude = log.Location?.Latitude,
            Longitude = log.Location?.Longitude,
            Remark = log.Annotations?.FirstOrDefault()?.Comment
        };
    }

    public static EldDriverHosDataDto MapToHosDto(string driverId, GeotabDutyStatusAvailability availability)
    {
        return new EldDriverHosDataDto
        {
            ExternalDriverId = driverId,
            CurrentDutyStatus = MapDutyStatus(availability.CurrentDutyStatus),
            StatusChangedAt = availability.CurrentDutyStatusStartDateTime ?? DateTime.UtcNow,
            DrivingMinutesRemaining = (int)(availability.Driving?.TotalMinutes ?? 0),
            OnDutyMinutesRemaining = (int)(availability.Duty?.TotalMinutes ?? 0),
            CycleMinutesRemaining = (int)(availability.Cycle?.TotalMinutes ?? 0),
            TimeUntilBreakRequired = availability.Rest,
            IsInViolation = availability.IsInViolation ?? false
        };
    }

    public static EldViolationDataDto MapToViolationDto(GeotabDutyStatusViolation v, Region region)
    {
        return new EldViolationDataDto
        {
            ExternalViolationId = v.Id,
            ExternalDriverId = v.Driver?.Id ?? "",
            ViolationDate = v.DateTime ?? DateTime.UtcNow,
            ViolationType = MapViolationType(v.ViolationType, region),
            Description = v.Description ?? v.ViolationType ?? "Unknown violation",
            SeverityLevel = 3
        };
    }

    public static DutyStatus MapDutyStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "off" or "off_duty" or "offduty" => DutyStatus.OffDuty,
            "sleeperberth" or "sleeper_berth" or "sleeper" => DutyStatus.SleeperBerth,
            "d" or "driving" => DutyStatus.Driving,
            "on" or "onduty" or "on_duty" => DutyStatus.OnDutyNotDriving,
            "yardmove" or "yard_move" or "ym" => DutyStatus.YardMove,
            "personalconveyance" or "personal_conveyance" or "pc" => DutyStatus.PersonalConveyance,
            _ => DutyStatus.OffDuty
        };
    }

    /// <summary>
    /// Region-aware. Geotab's <c>violationType</c> strings overlap between US (FMCSA)
    /// and EU (561/2006); the same enum value lives in different families. Default
    /// branches favour the region's most common form-and-manner type.
    /// </summary>
    public static HosViolationType MapViolationType(string? type, Region region)
    {
        var key = type?.ToLowerInvariant();

        if (region == Region.EU)
        {
            return key switch
            {
                "continuousdriving" or "continuous_driving" or "4_5_hour" => HosViolationType.EuContinuousDriving4_5h,
                "dailydriving" or "daily_driving" or "9_hour" => HosViolationType.EuDailyDriving9h,
                "weeklydriving" or "weekly_driving" or "56_hour" => HosViolationType.EuWeeklyDriving56h,
                "biweeklydriving" or "biweekly_driving" or "90_hour" => HosViolationType.EuBiweeklyDriving90h,
                "dailyrest" or "daily_rest" or "11_hour_rest" => HosViolationType.EuDailyRest11h,
                "weeklyrest" or "weekly_rest" or "45_hour_rest" => HosViolationType.EuWeeklyRest45h,
                _ => HosViolationType.EuFormAndManner
            };
        }

        return key switch
        {
            "driving" or "11_hour" => HosViolationType.Driving11Hour,
            "shift" or "14_hour" or "duty" => HosViolationType.OnDuty14Hour,
            "break" or "30_minute" => HosViolationType.Break30Minute,
            "cycle" or "70_hour" or "60_hour" => HosViolationType.Cycle70Hour,
            "restart" or "34_hour" => HosViolationType.RestartRequired,
            _ => HosViolationType.FormAndMannerViolation
        };
    }
}
