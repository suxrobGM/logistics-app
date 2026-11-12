using Logistics.Domain.Entities.Maintenance;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class MaintenanceMapper
{
    [MapperIgnoreSource(nameof(MaintenanceRecord.Truck))]
    [MapperIgnoreSource(nameof(MaintenanceRecord.PerformedBy))]
    [MapperIgnoreSource(nameof(MaintenanceRecord.Parts))]
    [MapperIgnoreTarget(nameof(MaintenanceRecordDto.TruckNumber))]
    [MapperIgnoreTarget(nameof(MaintenanceRecordDto.PerformedByName))]
    [MapperIgnoreTarget(nameof(MaintenanceRecordDto.Parts))]
    [MapProperty(nameof(MaintenanceRecord.MaintenanceType), nameof(MaintenanceRecordDto.Type))]
    [MapProperty(nameof(MaintenanceRecord.MaintenanceType), nameof(MaintenanceRecordDto.TypeDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(MaintenanceRecord.Description), nameof(MaintenanceRecordDto.Description), Use = nameof(@MappingConversions.DefaultToEmpty))]
    [MapProperty(nameof(MaintenanceRecord.WorkPerformed), nameof(MaintenanceRecordDto.Notes))]
    private static partial MaintenanceRecordDto MapMaintenanceRecord(MaintenanceRecord entity);

    [MapperIgnoreSource(nameof(MaintenancePart.MaintenanceRecord))]
    [MapProperty(nameof(MaintenancePart.PartNumber), nameof(MaintenancePartDto.PartNumber), Use = nameof(@MappingConversions.DefaultToEmpty))]
    public static partial MaintenancePartDto ToDto(this MaintenancePart entity);

    extension(MaintenanceRecord entity)
    {
        /// <summary>For one row: reads the related names off the navigations. Lists pass them in
        /// through the overload instead, looked up for the whole page at once.</summary>
        public MaintenanceRecordDto ToDto() => entity.ToDto(
            entity.Truck?.Number, entity.PerformedBy?.GetFullName(), entity.Parts.Select(p => p.ToDto()));

        public MaintenanceRecordDto ToDto(
            string? truckNumber, string? performedByName, IEnumerable<MaintenancePartDto> parts)
        {
            var dto = MapMaintenanceRecord(entity);
            dto.TruckNumber = truckNumber ?? string.Empty;
            dto.PerformedByName = performedByName;
            dto.Parts = parts.ToList();
            return dto;
        }
    }
}
