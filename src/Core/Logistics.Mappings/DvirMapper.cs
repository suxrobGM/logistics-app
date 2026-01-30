using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class DvirMapper
{
    public static DvirReportDto ToDto(this DvirReport entity)
    {
        return new DvirReportDto
        {
            Id = entity.Id,
            TruckId = entity.TruckId,
            TruckNumber = entity.Truck?.Number ?? string.Empty,
            DriverId = entity.DriverId,
            DriverName = entity.Driver?.GetFullName() ?? string.Empty,
            Type = entity.Type,
            Status = entity.Status,
            InspectionDate = entity.InspectionDate,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            OdometerReading = entity.OdometerReading,
            HasDefects = entity.HasDefects,
            HasDriverSignature = !string.IsNullOrEmpty(entity.DriverSignature),
            DriverNotes = entity.DriverNotes,
            ReviewedById = entity.ReviewedById,
            ReviewedByName = entity.ReviewedBy?.GetFullName(),
            ReviewedAt = entity.ReviewedAt,
            HasMechanicSignature = !string.IsNullOrEmpty(entity.MechanicSignature),
            MechanicNotes = entity.MechanicNotes,
            DefectsCorrected = entity.DefectsCorrected,
            TripId = entity.TripId,
            Defects = entity.Defects.Select(d => d.ToDto()).ToList(),
            Photos = entity.Photos.Select(p => p.ToDto()).ToList(),
            CreatedAt = entity.CreatedAt
        };
    }

    public static DvirDefectDto ToDto(this DvirDefect entity)
    {
        return new DvirDefectDto
        {
            Id = entity.Id,
            Category = entity.Category,
            CategoryDisplay = entity.Category.GetDescription(),
            Description = entity.Description,
            Severity = entity.Severity,
            SeverityDisplay = entity.Severity.GetDescription(),
            IsCorrected = entity.IsCorrected,
            CorrectionNotes = entity.CorrectionNotes,
            CorrectedAt = entity.CorrectedAt,
            CorrectedById = entity.CorrectedById,
            CorrectedByName = entity.CorrectedBy?.GetFullName()
        };
    }
}
