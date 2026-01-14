using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class EldMapper
{
    public static EldProviderConfigurationDto ToDto(this EldProviderConfiguration entity, int mappedDriversCount = 0,
        int mappedVehiclesCount = 0)
    {
        return new EldProviderConfigurationDto
        {
            Id = entity.Id,
            ProviderType = entity.ProviderType,
            ProviderName = GetProviderName(entity.ProviderType),
            IsActive = entity.IsActive,
            LastSyncedAt = entity.LastSyncedAt,
            IsConnected = entity.IsActive && !string.IsNullOrEmpty(entity.ApiKey),
            MappedDriversCount = mappedDriversCount,
            MappedVehiclesCount = mappedVehiclesCount
        };
    }

    public static DriverHosStatusDto ToDto(this DriverHosStatus entity, string? employeeName = null)
    {
        return new DriverHosStatusDto
        {
            EmployeeId = entity.EmployeeId,
            EmployeeName = employeeName ?? entity.Employee?.GetFullName(),
            CurrentDutyStatus = entity.CurrentDutyStatus,
            CurrentDutyStatusDisplay = GetDutyStatusDisplay(entity.CurrentDutyStatus),
            StatusChangedAt = entity.StatusChangedAt,
            DrivingMinutesRemaining = entity.DrivingMinutesRemaining,
            OnDutyMinutesRemaining = entity.OnDutyMinutesRemaining,
            CycleMinutesRemaining = entity.CycleMinutesRemaining,
            DrivingTimeRemainingDisplay = FormatMinutes(entity.DrivingMinutesRemaining),
            OnDutyTimeRemainingDisplay = FormatMinutes(entity.OnDutyMinutesRemaining),
            CycleTimeRemainingDisplay = FormatMinutes(entity.CycleMinutesRemaining),
            TimeUntilBreakRequired = entity.TimeUntilBreakRequired,
            IsInViolation = entity.IsInViolation,
            IsAvailableForDispatch = entity.IsAvailableForDispatch(),
            LastUpdatedAt = entity.LastUpdatedAt,
            NextMandatoryBreakAt = entity.NextMandatoryBreakAt,
            ProviderType = entity.ProviderType
        };
    }

    public static EldDriverMappingDto ToDto(this EldDriverMapping entity, string? employeeName = null)
    {
        return new EldDriverMappingDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = employeeName ?? entity.Employee?.GetFullName(),
            ProviderType = entity.ProviderType,
            ExternalDriverId = entity.ExternalDriverId,
            ExternalDriverName = entity.ExternalDriverName,
            IsSyncEnabled = entity.IsSyncEnabled,
            LastSyncedAt = entity.LastSyncedAt
        };
    }

    public static EldVehicleMappingDto ToDto(this EldVehicleMapping entity, string? truckNumber = null)
    {
        return new EldVehicleMappingDto
        {
            Id = entity.Id,
            TruckId = entity.TruckId,
            TruckNumber = truckNumber ?? entity.Truck?.Number,
            ProviderType = entity.ProviderType,
            ExternalVehicleId = entity.ExternalVehicleId,
            ExternalVehicleName = entity.ExternalVehicleName,
            IsSyncEnabled = entity.IsSyncEnabled,
            LastSyncedAt = entity.LastSyncedAt
        };
    }

    public static HosLogDto ToDto(this HosLog entity, string? employeeName = null)
    {
        return new HosLogDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = employeeName ?? entity.Employee?.GetFullName(),
            LogDate = entity.LogDate,
            DutyStatus = entity.DutyStatus,
            DutyStatusDisplay = GetDutyStatusDisplay(entity.DutyStatus),
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            DurationMinutes = entity.DurationMinutes,
            DurationDisplay = FormatMinutes(entity.DurationMinutes),
            Location = entity.Location,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Remark = entity.Remark,
            ProviderType = entity.ProviderType
        };
    }

    public static HosViolationDto ToDto(this HosViolation entity, string? employeeName = null)
    {
        return new HosViolationDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = employeeName ?? entity.Employee?.GetFullName(),
            ViolationDate = entity.ViolationDate,
            ViolationType = entity.ViolationType,
            ViolationTypeDisplay = GetViolationTypeDisplay(entity.ViolationType),
            Description = entity.Description,
            SeverityLevel = entity.SeverityLevel,
            IsResolved = entity.IsResolved,
            ResolvedAt = entity.ResolvedAt,
            ProviderType = entity.ProviderType
        };
    }

    private static string GetProviderName(EldProviderType providerType)
    {
        return providerType switch
        {
            EldProviderType.Samsara => "Samsara",
            EldProviderType.Motive => "Motive (KeepTruckin)",
            EldProviderType.Geotab => "Geotab",
            EldProviderType.Omnitracs => "Omnitracs",
            EldProviderType.PeopleNet => "PeopleNet",
            _ => "Unknown"
        };
    }

    private static string GetDutyStatusDisplay(DutyStatus status)
    {
        return status switch
        {
            DutyStatus.OffDuty => "Off Duty",
            DutyStatus.SleeperBerth => "Sleeper Berth",
            DutyStatus.Driving => "Driving",
            DutyStatus.OnDutyNotDriving => "On Duty (Not Driving)",
            DutyStatus.YardMove => "Yard Move",
            DutyStatus.PersonalConveyance => "Personal Conveyance",
            _ => "Unknown"
        };
    }

    private static string GetViolationTypeDisplay(HosViolationType violationType)
    {
        return violationType switch
        {
            HosViolationType.Driving11Hour => "11-Hour Driving Limit",
            HosViolationType.OnDuty14Hour => "14-Hour On-Duty Limit",
            HosViolationType.Break30Minute => "30-Minute Break Required",
            HosViolationType.Cycle70Hour => "70-Hour/8-Day Cycle Limit",
            HosViolationType.RestartRequired => "34-Hour Restart Required",
            HosViolationType.FormAndMannerViolation => "Form and Manner Violation",
            _ => "Unknown"
        };
    }

    private static string FormatMinutes(int totalMinutes)
    {
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return $"{hours}h {minutes}m";
    }
}
