using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class EldMapper
{
    extension(EldProviderConfiguration entity)
    {
        public EldProviderConfigurationDto ToDto(int mappedDriversCount = 0,
            int mappedVehiclesCount = 0)
        {
            return new EldProviderConfigurationDto
            {
                Id = entity.Id,
                ProviderType = entity.ProviderType,
                ProviderName = entity.ProviderType.GetDescription(),
                IsActive = entity.IsActive,
                LastSyncedAt = entity.LastSyncedAt,
                IsConnected = entity.IsActive && !string.IsNullOrEmpty(entity.ApiKey),
                MappedDriversCount = mappedDriversCount,
                MappedVehiclesCount = mappedVehiclesCount,
                ExternalAccountId = entity.ExternalAccountId,
                Region = entity.Region,
                HasWebhookSecret = !string.IsNullOrEmpty(entity.WebhookSecret)
            };
        }
    }

    extension(DriverHosStatus entity)
    {
        public DriverHosStatusDto ToDto(string? employeeName = null)
        {
            return new DriverHosStatusDto
            {
                EmployeeId = entity.EmployeeId,
                EmployeeName = employeeName ?? entity.Employee?.GetFullName(),
                CurrentDutyStatus = entity.CurrentDutyStatus,
                CurrentDutyStatusDisplay = entity.CurrentDutyStatus.GetDescription(),
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
                NextMandatoryBreakAt = entity.NextMandatoryBreakAt
            };
        }
    }

    extension(EldDriverMapping entity)
    {
        public EldDriverMappingDto ToDto(string? employeeName = null)
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
    }

    extension(EldVehicleMapping entity)
    {
        public EldVehicleMappingDto ToDto(string? truckNumber = null)
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
    }

    extension(HosLog entity)
    {
        public HosLogDto ToDto(string? employeeName = null)
        {
            return new HosLogDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = employeeName ?? entity.Employee?.GetFullName(),
                LogDate = entity.LogDate,
                DutyStatus = entity.DutyStatus,
                DutyStatusDisplay = entity.DutyStatus.GetDescription(),
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
    }

    extension(HosViolation entity)
    {
        public HosViolationDto ToDto(string? employeeName = null)
        {
            return new HosViolationDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = employeeName ?? entity.Employee?.GetFullName(),
                ViolationDate = entity.ViolationDate,
                ViolationType = entity.ViolationType,
                ViolationTypeDisplay = entity.ViolationType.GetDescription(),
                Description = entity.Description,
                SeverityLevel = entity.SeverityLevel,
                IsResolved = entity.IsResolved,
                ResolvedAt = entity.ResolvedAt,
                ProviderType = entity.ProviderType,
                RuleSetCode = entity.RuleSetCode
            };
        }
    }

    extension(HosLimits limits)
    {
        public HosLimitsDto ToDto()
        {
            return new HosLimitsDto
            {
                RuleSetCode = limits.RuleSetCode,
                MaxDailyDrivingMinutes = limits.MaxDailyDrivingMinutes,
                MaxDailyOnDutyMinutes = limits.MaxDailyOnDutyMinutes,
                MaxWeeklyDrivingMinutes = limits.MaxWeeklyDrivingMinutes,
                MaxBiweeklyDrivingMinutes = limits.MaxBiweeklyDrivingMinutes,
                MaxContinuousDrivingMinutes = limits.MaxContinuousDrivingMinutes,
                MinDailyRestMinutes = limits.MinDailyRestMinutes,
                MinWeeklyRestMinutes = limits.MinWeeklyRestMinutes,
                RequiredBreakAfterMinutes = limits.RequiredBreakAfterMinutes,
                CycleDays = limits.CycleDays
            };
        }
    }

    private static string FormatMinutes(int totalMinutes)
    {
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return $"{hours}h {minutes}m";
    }
}
