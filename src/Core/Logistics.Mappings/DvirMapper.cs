using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums.Safety;
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
            CategoryDisplay = GetCategoryDisplay(entity.Category),
            Description = entity.Description,
            Severity = entity.Severity,
            SeverityDisplay = GetSeverityDisplay(entity.Severity),
            IsCorrected = entity.IsCorrected,
            CorrectionNotes = entity.CorrectionNotes,
            CorrectedAt = entity.CorrectedAt,
            CorrectedById = entity.CorrectedById,
            CorrectedByName = entity.CorrectedBy?.GetFullName()
        };
    }

    private static string GetCategoryDisplay(DvirInspectionCategory category)
    {
        return category switch
        {
            DvirInspectionCategory.AirCompressor => "Air Compressor",
            DvirInspectionCategory.AirLines => "Air Lines",
            DvirInspectionCategory.Battery => "Battery",
            DvirInspectionCategory.BrakesService => "Brakes - Service",
            DvirInspectionCategory.BrakesParking => "Brakes - Parking",
            DvirInspectionCategory.Clutch => "Clutch",
            DvirInspectionCategory.CouplingDevices => "Coupling Devices",
            DvirInspectionCategory.DefrosterHeater => "Defroster/Heater",
            DvirInspectionCategory.DriveLine => "Drive Line",
            DvirInspectionCategory.Engine => "Engine",
            DvirInspectionCategory.Exhaust => "Exhaust System",
            DvirInspectionCategory.FifthWheel => "Fifth Wheel",
            DvirInspectionCategory.FluidLevels => "Fluid Levels",
            DvirInspectionCategory.Frame => "Frame and Assembly",
            DvirInspectionCategory.FrontAxle => "Front Axle",
            DvirInspectionCategory.FuelSystem => "Fuel System",
            DvirInspectionCategory.Horn => "Horn",
            DvirInspectionCategory.LightsHead => "Lights - Head",
            DvirInspectionCategory.LightsTail => "Lights - Tail",
            DvirInspectionCategory.LightsBrake => "Lights - Brake",
            DvirInspectionCategory.LightsTurn => "Lights - Turn Signals",
            DvirInspectionCategory.LightsMarker => "Lights - Clearance/Marker",
            DvirInspectionCategory.Mirrors => "Mirrors",
            DvirInspectionCategory.Muffler => "Muffler",
            DvirInspectionCategory.OilPressure => "Oil Pressure",
            DvirInspectionCategory.Radiator => "Radiator",
            DvirInspectionCategory.RearEnd => "Rear End",
            DvirInspectionCategory.Reflectors => "Reflectors",
            DvirInspectionCategory.SafetyEquipment => "Safety Equipment",
            DvirInspectionCategory.SeatBelts => "Seat Belts",
            DvirInspectionCategory.Speedometer => "Speedometer",
            DvirInspectionCategory.Springs => "Springs",
            DvirInspectionCategory.Starter => "Starter",
            DvirInspectionCategory.Steering => "Steering",
            DvirInspectionCategory.Suspension => "Suspension",
            DvirInspectionCategory.Tires => "Tires",
            DvirInspectionCategory.Transmission => "Transmission",
            DvirInspectionCategory.TripRecorder => "Trip Recorder/ELD",
            DvirInspectionCategory.Wheels => "Wheels and Rims",
            DvirInspectionCategory.Windows => "Windows",
            DvirInspectionCategory.Windshield => "Windshield",
            DvirInspectionCategory.Wipers => "Windshield Wipers",
            DvirInspectionCategory.Other => "Other",
            _ => "Unknown"
        };
    }

    private static string GetSeverityDisplay(DefectSeverity severity)
    {
        return severity switch
        {
            DefectSeverity.Minor => "Minor",
            DefectSeverity.Major => "Major",
            DefectSeverity.OutOfService => "Out of Service",
            _ => "Unknown"
        };
    }
}
