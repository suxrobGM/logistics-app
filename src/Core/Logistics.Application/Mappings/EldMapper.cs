using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class EldMapper
{
    [MapperIgnoreTarget(nameof(EldProviderConfigurationDto.MappedDriversCount))]
    [MapperIgnoreTarget(nameof(EldProviderConfigurationDto.MappedVehiclesCount))]
    [MapProperty(nameof(EldProviderConfiguration.ProviderType), nameof(EldProviderConfigurationDto.ProviderName), Use = nameof(@MappingConversions.Describe))]
    [MapPropertyFromSource(nameof(EldProviderConfigurationDto.IsConnected), Use = nameof(IsConnected))]
    [MapProperty(nameof(EldProviderConfiguration.WebhookSecret), nameof(EldProviderConfigurationDto.HasWebhookSecret), Use = nameof(@MappingConversions.HasText))]
    private static partial EldProviderConfigurationDto MapProviderConfiguration(EldProviderConfiguration entity);

    [MapperIgnoreSource(nameof(DriverHosStatus.Employee))]
    [MapperIgnoreTarget(nameof(DriverHosStatusDto.EmployeeName))]
    [MapperIgnoreTarget(nameof(DriverHosStatusDto.IsAvailableForDispatch))]
    [MapProperty(nameof(DriverHosStatus.CurrentDutyStatus), nameof(DriverHosStatusDto.CurrentDutyStatusDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(DriverHosStatus.DrivingMinutesRemaining), nameof(DriverHosStatusDto.DrivingTimeRemainingDisplay), Use = nameof(FormatMinutes))]
    [MapProperty(nameof(DriverHosStatus.OnDutyMinutesRemaining), nameof(DriverHosStatusDto.OnDutyTimeRemainingDisplay), Use = nameof(FormatMinutes))]
    [MapProperty(nameof(DriverHosStatus.CycleMinutesRemaining), nameof(DriverHosStatusDto.CycleTimeRemainingDisplay), Use = nameof(FormatMinutes))]
    private static partial DriverHosStatusDto MapHosStatus(DriverHosStatus entity);

    [MapperIgnoreSource(nameof(EldDriverMapping.Employee))]
    [MapperIgnoreTarget(nameof(EldDriverMappingDto.EmployeeName))]
    private static partial EldDriverMappingDto MapDriverMapping(EldDriverMapping entity);

    [MapperIgnoreSource(nameof(EldVehicleMapping.Truck))]
    [MapperIgnoreTarget(nameof(EldVehicleMappingDto.TruckNumber))]
    private static partial EldVehicleMappingDto MapVehicleMapping(EldVehicleMapping entity);

    [MapperIgnoreSource(nameof(HosLog.Employee))]
    [MapperIgnoreTarget(nameof(HosLogDto.EmployeeName))]
    [MapProperty(nameof(HosLog.DutyStatus), nameof(HosLogDto.DutyStatusDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(HosLog.DurationMinutes), nameof(HosLogDto.DurationDisplay), Use = nameof(FormatMinutes))]
    private static partial HosLogDto MapHosLog(HosLog entity);

    [MapperIgnoreSource(nameof(HosViolation.Employee))]
    [MapperIgnoreTarget(nameof(HosViolationDto.EmployeeName))]
    [MapProperty(nameof(HosViolation.ViolationType), nameof(HosViolationDto.ViolationTypeDisplay), Use = nameof(@MappingConversions.Describe))]
    private static partial HosViolationDto MapHosViolation(HosViolation entity);

    public static partial HosLimitsDto ToDto(this HosLimits limits);

    extension(EldProviderConfiguration entity)
    {
        public EldProviderConfigurationDto ToDto(int mappedDriversCount = 0,
            int mappedVehiclesCount = 0)
        {
            var dto = MapProviderConfiguration(entity);
            return dto with { MappedDriversCount = mappedDriversCount, MappedVehiclesCount = mappedVehiclesCount };
        }
    }

    extension(DriverHosStatus entity)
    {
        public DriverHosStatusDto ToDto(string? employeeName = null)
        {
            var dto = MapHosStatus(entity);
            return dto with
            {
                EmployeeName = employeeName ?? entity.Employee?.GetFullName(),
                IsAvailableForDispatch = entity.IsAvailableForDispatch()
            };
        }
    }

    extension(EldDriverMapping entity)
    {
        public EldDriverMappingDto ToDto(string? employeeName = null)
        {
            var dto = MapDriverMapping(entity);
            return dto with { EmployeeName = employeeName ?? entity.Employee?.GetFullName() };
        }
    }

    extension(EldVehicleMapping entity)
    {
        public EldVehicleMappingDto ToDto(string? truckNumber = null)
        {
            var dto = MapVehicleMapping(entity);
            return dto with { TruckNumber = truckNumber ?? entity.Truck?.Number };
        }
    }

    extension(HosLog entity)
    {
        public HosLogDto ToDto(string? employeeName = null)
        {
            var dto = MapHosLog(entity);
            return dto with { EmployeeName = employeeName ?? entity.Employee?.GetFullName() };
        }
    }

    extension(HosViolation entity)
    {
        public HosViolationDto ToDto(string? employeeName = null)
        {
            var dto = MapHosViolation(entity);
            return dto with { EmployeeName = employeeName ?? entity.Employee?.GetFullName() };
        }
    }

    private static bool IsConnected(EldProviderConfiguration entity) =>
        entity.IsActive && !string.IsNullOrEmpty(entity.ApiKey);

    private static string FormatMinutes(int totalMinutes)
    {
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return $"{hours}h {minutes}m";
    }
}
