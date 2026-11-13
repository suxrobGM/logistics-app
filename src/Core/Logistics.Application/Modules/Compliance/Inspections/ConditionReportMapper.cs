using Logistics.Application.Common.Mapping;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Compliance.Inspections;

[Mapper]
public static partial class ConditionReportMapper
{
    [MapperIgnoreSource(nameof(ConditionDefect.LoadConditionReport))]
    [MapProperty(nameof(ConditionDefect.PartCategory), nameof(ConditionDefectDto.PartCategoryDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(ConditionDefect.Severity), nameof(ConditionDefectDto.SeverityDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(ConditionDefect.Zone), nameof(ConditionDefectDto.ZoneDisplay), Use = nameof(DescribeZone))]
    [MapProperty(nameof(ConditionDefect.DamageType), nameof(ConditionDefectDto.DamageCode), Use = nameof(GetDamageCode))]
    public static partial ConditionDefectDto ToDto(this ConditionDefect entity);

    private static string? DescribeZone(VehicleDamageZone? zone) => zone?.GetDescription();

    private static string? GetDamageCode(VehicleDamageType? type) => type?.GetCode();
}
