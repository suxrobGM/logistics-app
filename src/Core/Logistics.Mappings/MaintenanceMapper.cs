using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class MaintenanceMapper
{
    extension(MaintenanceRecord entity)
    {
        /// <summary>For one row: reads the related names off the navigations. Lists pass them in
        /// through the overload instead, looked up for the whole page at once.</summary>
        public MaintenanceRecordDto ToDto() => entity.ToDto(
            entity.Truck?.Number, entity.PerformedBy?.GetFullName(), entity.Parts.Select(p => p.ToDto()));

        public MaintenanceRecordDto ToDto(
            string? truckNumber, string? performedByName, IEnumerable<MaintenancePartDto> parts)
        {
            return new MaintenanceRecordDto
            {
                Id = entity.Id,
                TruckId = entity.TruckId,
                TruckNumber = truckNumber ?? string.Empty,
                Type = entity.MaintenanceType,
                TypeDisplay = entity.MaintenanceType.GetDescription(),
                Description = entity.Description ?? string.Empty,
                ServiceDate = entity.ServiceDate,
                OdometerReading = entity.OdometerReading,
                EngineHours = entity.EngineHours,
                VendorName = entity.VendorName,
                InvoiceNumber = entity.InvoiceNumber,
                LaborCost = entity.LaborCost,
                PartsCost = entity.PartsCost,
                TotalCost = entity.TotalCost,
                PerformedById = entity.PerformedById,
                PerformedByName = performedByName,
                Notes = entity.WorkPerformed,
                Parts = parts.ToList(),
                CreatedAt = entity.CreatedAt
            };
        }
    }

    extension(MaintenancePart entity)
    {
        public MaintenancePartDto ToDto()
        {
            return new MaintenancePartDto
            {
                Id = entity.Id,
                PartNumber = entity.PartNumber ?? string.Empty,
                PartName = entity.PartName,
                Quantity = entity.Quantity,
                UnitCost = entity.UnitCost,
                TotalCost = entity.TotalCost
            };
        }
    }
}
