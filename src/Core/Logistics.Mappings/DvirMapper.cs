using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class DvirMapper
{
    extension(DvirReport entity)
    {
        /// <summary>For one row: reads the related names off the navigations. Lists pass them in
        /// through the overload instead, looked up for the whole page at once.</summary>
        public DvirReportDto ToDto() => entity.ToDto(
            entity.Truck?.Number,
            entity.Driver?.GetFullName(),
            entity.ReviewedBy?.GetFullName(),
            entity.Defects.Select(d => d.ToDto()));

        public DvirReportDto ToDto(
            string? truckNumber, string? driverName, string? reviewerName, IEnumerable<DvirDefectDto> defects)
        {
            return new DvirReportDto
            {
                Id = entity.Id,
                TruckId = entity.TruckId,
                TruckNumber = truckNumber ?? string.Empty,
                DriverId = entity.DriverId,
                DriverName = driverName ?? string.Empty,
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
                ReviewedByName = reviewerName,
                ReviewedAt = entity.ReviewedAt,
                HasMechanicSignature = !string.IsNullOrEmpty(entity.MechanicSignature),
                MechanicNotes = entity.MechanicNotes,
                DefectsCorrected = entity.DefectsCorrected,
                TripId = entity.TripId,
                Defects = defects.ToList(),
                CreatedAt = entity.CreatedAt
            };
        }
    }

    extension(DvirDefect entity)
    {
        public DvirDefectDto ToDto() => entity.ToDto(entity.CorrectedBy?.GetFullName());

        public DvirDefectDto ToDto(string? correctedByName)
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
                CorrectedByName = correctedByName
            };
        }
    }
}
