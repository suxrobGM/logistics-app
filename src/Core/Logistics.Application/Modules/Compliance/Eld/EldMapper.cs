using Logistics.Application.Common.Mapping;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Compliance.Eld;

[Mapper]
public static partial class EldMapper
{
    [MapperIgnoreTarget(nameof(EldProviderConfigurationDto.MappedDriversCount))]
    [MapperIgnoreTarget(nameof(EldProviderConfigurationDto.MappedVehiclesCount))]
    [MapProperty(nameof(EldProviderConfiguration.ProviderType), nameof(EldProviderConfigurationDto.ProviderName), Use = nameof(@MappingConversions.Describe))]
    [MapPropertyFromSource(nameof(EldProviderConfigurationDto.IsConnected), Use = nameof(IsConnected))]
    [MapProperty(nameof(EldProviderConfiguration.WebhookSecret), nameof(EldProviderConfigurationDto.HasWebhookSecret), Use = nameof(@MappingConversions.HasText))]
    private static partial EldProviderConfigurationDto MapProviderConfiguration(EldProviderConfiguration entity);

    [MapperIgnoreSource(nameof(HosLog.Employee))]
    [MapperIgnoreTarget(nameof(HosLogDto.EmployeeName))]
    [MapProperty(nameof(HosLog.DutyStatus), nameof(HosLogDto.DutyStatusDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(HosLog.DurationMinutes), nameof(HosLogDto.DurationDisplay), Use = nameof(FormatMinutes))]
    private static partial HosLogDto MapHosLog(HosLog entity);

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

    extension(HosLog entity)
    {
        public HosLogDto ToDto(string? employeeName = null)
        {
            var dto = MapHosLog(entity);
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
