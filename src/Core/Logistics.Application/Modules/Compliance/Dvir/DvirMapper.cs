using Logistics.Application.Common.Mapping;
using Logistics.Domain.Entities.Safety;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Compliance.Dvir;

[Mapper]
public static partial class DvirMapper
{
    [MapperIgnoreSource(nameof(DvirReport.Truck))]
    [MapperIgnoreSource(nameof(DvirReport.Driver))]
    [MapperIgnoreSource(nameof(DvirReport.ReviewedBy))]
    [MapperIgnoreSource(nameof(DvirReport.Trip))]
    [MapperIgnoreSource(nameof(DvirReport.Defects))]
    [MapperIgnoreTarget(nameof(DvirReportDto.TruckNumber))]
    [MapperIgnoreTarget(nameof(DvirReportDto.DriverName))]
    [MapperIgnoreTarget(nameof(DvirReportDto.ReviewedByName))]
    [MapperIgnoreTarget(nameof(DvirReportDto.Defects))]
    [MapProperty(nameof(DvirReport.DriverSignature), nameof(DvirReportDto.HasDriverSignature), Use = nameof(@MappingConversions.HasText))]
    [MapProperty(nameof(DvirReport.MechanicSignature), nameof(DvirReportDto.HasMechanicSignature), Use = nameof(@MappingConversions.HasText))]
    private static partial DvirReportDto MapDvirReport(DvirReport entity);

    [MapperIgnoreSource(nameof(DvirDefect.DvirReport))]
    [MapperIgnoreSource(nameof(DvirDefect.CorrectedBy))]
    [MapperIgnoreTarget(nameof(DvirDefectDto.CorrectedByName))]
    [MapProperty(nameof(DvirDefect.Category), nameof(DvirDefectDto.CategoryDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(DvirDefect.Severity), nameof(DvirDefectDto.SeverityDisplay), Use = nameof(@MappingConversions.Describe))]
    private static partial DvirDefectDto MapDvirDefect(DvirDefect entity);

    extension(DvirReport entity)
    {
        /// <summary>Single rows only; lists batch the names and use the overload.</summary>
        public DvirReportDto ToDto() => entity.ToDto(
            entity.Truck?.Number,
            entity.Driver?.GetFullName(),
            entity.ReviewedBy?.GetFullName(),
            entity.Defects.Select(d => d.ToDto()));

        public DvirReportDto ToDto(
            string? truckNumber, string? driverName, string? reviewerName, IEnumerable<DvirDefectDto> defects)
        {
            var dto = MapDvirReport(entity);
            dto.TruckNumber = truckNumber ?? string.Empty;
            dto.DriverName = driverName ?? string.Empty;
            dto.ReviewedByName = reviewerName;
            dto.Defects = defects.ToList();
            return dto;
        }
    }

    extension(DvirDefect entity)
    {
        public DvirDefectDto ToDto() => entity.ToDto(entity.CorrectedBy?.GetFullName());

        public DvirDefectDto ToDto(string? correctedByName)
        {
            var dto = MapDvirDefect(entity);
            dto.CorrectedByName = correctedByName;
            return dto;
        }
    }
}
