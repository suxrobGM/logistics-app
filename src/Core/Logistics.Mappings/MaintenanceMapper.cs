using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Primitives.Enums.Maintenance;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class MaintenanceMapper
{
    public static MaintenanceRecordDto ToDto(this MaintenanceRecord entity)
    {
        return new MaintenanceRecordDto
        {
            Id = entity.Id,
            TruckId = entity.TruckId,
            TruckNumber = entity.Truck?.Number ?? string.Empty,
            Type = entity.MaintenanceType,
            TypeDisplay = GetMaintenanceTypeDisplay(entity.MaintenanceType),
            Description = entity.Description,
            ServiceDate = entity.ServiceDate,
            OdometerReading = entity.OdometerReading,
            EngineHours = entity.EngineHours,
            VendorName = entity.VendorName,
            InvoiceNumber = entity.InvoiceNumber,
            LaborCost = entity.LaborCost,
            PartsCost = entity.PartsCost,
            TotalCost = entity.TotalCost,
            PerformedById = entity.PerformedById,
            PerformedByName = entity.PerformedBy?.GetFullName(),
            Notes = entity.WorkPerformed,
            Parts = entity.Parts.Select(p => p.ToDto()).ToList(),
            Documents = entity.Documents.Select(d => d.ToDto()).ToList(),
            CreatedAt = entity.CreatedAt
        };
    }

    public static MaintenancePartDto ToDto(this MaintenancePart entity)
    {
        return new MaintenancePartDto
        {
            Id = entity.Id,
            PartNumber = entity.PartNumber,
            PartName = entity.PartName,
            Quantity = entity.Quantity,
            UnitCost = entity.UnitCost,
            TotalCost = entity.TotalCost
        };
    }

    public static MaintenanceScheduleDto ToDto(this MaintenanceSchedule entity, int? currentOdometer = null)
    {
        var isOverdue = false;
        int? milesUntilDue = null;
        int? daysUntilDue = null;

        if (entity.NextDueMileage.HasValue && currentOdometer.HasValue)
        {
            milesUntilDue = entity.NextDueMileage.Value - currentOdometer.Value;
            if (milesUntilDue < 0) isOverdue = true;
        }

        if (entity.NextDueDate.HasValue)
        {
            daysUntilDue = (entity.NextDueDate.Value - DateTime.UtcNow).Days;
            if (daysUntilDue < 0) isOverdue = true;
        }

        return new MaintenanceScheduleDto
        {
            Id = entity.Id,
            TruckId = entity.TruckId,
            TruckNumber = entity.Truck?.Number ?? string.Empty,
            Type = entity.MaintenanceType,
            TypeDisplay = GetMaintenanceTypeDisplay(entity.MaintenanceType),
            IntervalType = entity.IntervalType,
            MileageInterval = entity.MileageInterval,
            DaysInterval = entity.DaysInterval,
            EngineHoursInterval = entity.EngineHoursInterval,
            LastServiceOdometer = entity.LastServiceMileage,
            LastServiceDate = entity.LastServiceDate,
            LastServiceEngineHours = entity.LastServiceEngineHours,
            NextServiceOdometer = entity.NextDueMileage,
            NextServiceDate = entity.NextDueDate,
            NextServiceEngineHours = entity.NextDueEngineHours,
            IsOverdue = isOverdue,
            MilesUntilDue = milesUntilDue,
            DaysUntilDue = daysUntilDue,
            IsActive = entity.IsActive,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt
        };
    }

    private static string GetMaintenanceTypeDisplay(MaintenanceType type)
    {
        return type switch
        {
            MaintenanceType.OilChange => "Oil Change",
            MaintenanceType.TireRotation => "Tire Rotation",
            MaintenanceType.TireReplacement => "Tire Replacement",
            MaintenanceType.BrakeInspection => "Brake Inspection",
            MaintenanceType.BrakeReplacement => "Brake Replacement",
            MaintenanceType.AirFilterReplacement => "Air Filter Replacement",
            MaintenanceType.FuelFilterReplacement => "Fuel Filter Replacement",
            MaintenanceType.TransmissionService => "Transmission Service",
            MaintenanceType.CoolantFlush => "Coolant Flush",
            MaintenanceType.BeltInspection => "Belt Inspection",
            MaintenanceType.Battery => "Battery Check/Replacement",
            MaintenanceType.AnnualDotInspection => "Annual DOT Inspection",
            MaintenanceType.PreventiveMaintenance => "Preventive Maintenance",
            MaintenanceType.EngineService => "Engine Service",
            MaintenanceType.SuspensionService => "Suspension Service",
            MaintenanceType.ElectricalRepair => "Electrical Repair",
            MaintenanceType.BodyWork => "Body Work",
            MaintenanceType.HvacService => "HVAC Service",
            MaintenanceType.ExhaustSystem => "Exhaust System",
            MaintenanceType.SteeringRepair => "Steering Repair",
            MaintenanceType.Other => "Other",
            _ => "Unknown"
        };
    }
}
