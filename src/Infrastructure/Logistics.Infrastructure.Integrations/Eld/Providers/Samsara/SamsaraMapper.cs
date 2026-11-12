using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;

internal static class SamsaraMapper
{
    private const double MetersPerMile = 1609.344;

    /// <summary>Null when the clock has no driver id or no known duty status (driver app disconnected).</summary>
    public static EldDriverHosDataDto? MapToDto(SamsaraHosClock clock)
    {
        var status = MapDutyStatus(clock.CurrentDutyStatus?.HosStatusType);
        if (string.IsNullOrEmpty(clock.Driver?.Id) || status is null)
        {
            return null;
        }

        var breakMs = clock.Clocks?.Break?.TimeUntilBreakDurationMs;
        return new EldDriverHosDataDto
        {
            ExternalDriverId = clock.Driver.Id,
            ExternalDriverName = clock.Driver.Name,
            CurrentDutyStatus = status.Value,
            DrivingMinutesRemaining = ToMinutes(clock.Clocks?.Drive?.DriveRemainingDurationMs),
            OnDutyMinutesRemaining = ToMinutes(clock.Clocks?.Shift?.ShiftRemainingDurationMs),
            CycleMinutesRemaining = ToMinutes(clock.Clocks?.Cycle?.CycleRemainingDurationMs),
            TimeUntilBreakRequired = breakMs.HasValue ? TimeSpan.FromMilliseconds(breakMs.Value) : null,
            IsInViolation = clock.Violations?.ShiftDrivingViolationDurationMs > 0
                            || clock.Violations?.CycleViolationDurationMs > 0
        };
    }

    /// <summary>
    /// Samsara log entries carry no id, so the driver id plus start time stands in for one: an edit
    /// that moves the start time becomes a new entry and the sync drops the old one.
    /// </summary>
    public static EldHosLogEntryDto? MapToLogDto(string driverId, SamsaraHosLogEntry entry)
    {
        var status = MapDutyStatus(entry.HosStatusType);
        if (status is null)
        {
            return null;
        }

        var start = entry.LogStartTime.UtcDateTime;
        var end = entry.LogEndTime?.UtcDateTime;
        return new EldHosLogEntryDto
        {
            ExternalLogId = $"{driverId}:{entry.LogStartTime.ToUnixTimeMilliseconds()}",
            ExternalDriverId = driverId,
            LogDate = start.Date,
            DutyStatus = status.Value,
            StartTime = start,
            EndTime = end,
            DurationMinutes = end.HasValue ? (int)(end.Value - start).TotalMinutes : 0,
            Latitude = entry.LogRecordedLocation?.Latitude,
            Longitude = entry.LogRecordedLocation?.Longitude,
            Remark = entry.Remark
        };
    }

    public static EldViolationDataDto? MapToViolationDto(SamsaraViolation violation)
    {
        var type = MapViolationType(violation.Type);
        if (string.IsNullOrEmpty(violation.Driver?.Id) || violation.ViolationStartTime is null || type is null)
        {
            return null;
        }

        var start = violation.ViolationStartTime.Value;
        return new EldViolationDataDto
        {
            ExternalViolationId = $"{violation.Driver.Id}:{violation.Type}:{start.ToUnixTimeMilliseconds()}",
            ExternalDriverId = violation.Driver.Id,
            ViolationDate = start.UtcDateTime,
            ViolationType = type.Value,
            Description = violation.Description ?? violation.Type!,
            SeverityLevel = MapSeverity(type.Value)
        };
    }

    public static EldDriverDto MapToDriverDto(SamsaraDriver driver)
    {
        return new EldDriverDto
        {
            ExternalDriverId = driver.Id ?? "",
            Name = driver.Name ?? driver.Username ?? "",
            Email = driver.Email,
            Phone = driver.Phone,
            LicenseNumber = driver.LicenseNumber
        };
    }

    public static EldVehicleDto MapToVehicleDto(SamsaraVehicle vehicle)
    {
        return new EldVehicleDto
        {
            ExternalVehicleId = vehicle.Id ?? "",
            Name = vehicle.Name ?? "",
            Vin = vehicle.Vin,
            LicensePlate = vehicle.LicensePlate,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = int.TryParse(vehicle.Year, out var year) ? year : null
        };
    }

    /// <summary>Null for a vehicle without a GPS fix. OBD odometer wins over the GPS-derived one.</summary>
    public static EldVehicleLocationDto? MapToLocationDto(SamsaraVehicleStats stats)
    {
        if (string.IsNullOrEmpty(stats.Id) || stats.Gps is null)
        {
            return null;
        }

        var odometerMeters = stats.ObdOdometerMeters?.Value ?? stats.GpsOdometerMeters?.Value;
        return new EldVehicleLocationDto
        {
            ExternalVehicleId = stats.Id,
            TruckNumber = stats.Name,
            Latitude = stats.Gps.Latitude,
            Longitude = stats.Gps.Longitude,
            Timestamp = stats.Gps.Time.UtcDateTime,
            OdometerMiles = odometerMeters.HasValue ? (int)(odometerMeters.Value / MetersPerMile) : null
        };
    }

    /// <summary>
    /// Only events Samsara actually publishes: the 2.0 driver/vehicle events and an <c>AlertIncident</c> whose
    /// condition is an HOS violation. <c>Ping</c>, 1.0 <c>Alert</c> and every other alert map to Unknown.
    /// </summary>
    public static EldWebhookResultDto MapToWebhookResult(SamsaraWebhookPayload webhook)
    {
        var hosViolationDriver = webhook.Data?.Conditions?
            .Select(c => c.Details?.HosViolation?.Driver)
            .FirstOrDefault(d => !string.IsNullOrEmpty(d?.Id));

        var eventType = webhook.EventType switch
        {
            "DriverCreated" => EldWebhookEventType.DriverCreated,
            "DriverUpdated" => EldWebhookEventType.DriverUpdated,
            "VehicleCreated" => EldWebhookEventType.VehicleCreated,
            "VehicleUpdated" => EldWebhookEventType.VehicleUpdated,
            "AlertIncident" when hosViolationDriver is not null => webhook.Data?.IsResolved == true
                ? EldWebhookEventType.ViolationResolved
                : EldWebhookEventType.ViolationCreated,
            _ => EldWebhookEventType.Unknown
        };

        return new EldWebhookResultDto
        {
            EventType = eventType,
            ExternalEventId = webhook.EventId,
            ExternalDriverId = hosViolationDriver?.Id ?? webhook.Data?.Driver?.Id,
            ExternalVehicleId = webhook.Data?.Vehicle?.Id,
            IsValid = true
        };
    }

    public static DutyStatus? MapDutyStatus(string? hosStatusType)
    {
        return hosStatusType switch
        {
            "offDuty" => DutyStatus.OffDuty,
            "sleeperBed" => DutyStatus.SleeperBerth,
            "driving" => DutyStatus.Driving,
            "onDuty" => DutyStatus.OnDutyNotDriving,
            "yardMove" => DutyStatus.YardMove,
            "personalConveyance" => DutyStatus.PersonalConveyance,
            _ => null
        };
    }

    /// <summary>Samsara's violation <c>type</c> enum folded into our rule categories; <c>NONE</c> is null.</summary>
    public static HosViolationType? MapViolationType(string? type)
    {
        return type switch
        {
            "dailyDrivingHours" or "shiftDrivingHours" => HosViolationType.Driving11Hour,
            "shiftHours" or "shiftOnDutyHours" or "dailyOnDutyHours" => HosViolationType.OnDuty14Hour,
            "restbreakMissed" or "californiaMealbreakMissed" => HosViolationType.Break30Minute,
            "cycleHoursOn" or "cycleOffHoursAfterOnDutyHours" => HosViolationType.Cycle70Hour,
            "mandatory24HoursOffDuty" => HosViolationType.RestartRequired,
            null or "" or "NONE" => null,
            _ => HosViolationType.FormAndMannerViolation
        };
    }

    /// <summary>
    /// Samsara reports no severity. Ours follows the FMCSA CSA weighting: hours-limit breaches outrank
    /// a missed break, which outranks paperwork (unsubmitted logs, off-duty deferral rules).
    /// </summary>
    private static int MapSeverity(HosViolationType type)
    {
        return type switch
        {
            HosViolationType.Driving11Hour or HosViolationType.OnDuty14Hour or HosViolationType.Cycle70Hour => 4,
            HosViolationType.Break30Minute or HosViolationType.RestartRequired => 3,
            _ => 2
        };
    }

    private static int ToMinutes(long? milliseconds) => (int)((milliseconds ?? 0) / 60000);
}
