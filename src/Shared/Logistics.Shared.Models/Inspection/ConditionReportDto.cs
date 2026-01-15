using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class ConditionReportDto
{
    public Guid Id { get; set; }
    public Guid LoadId { get; set; }
    public string Vin { get; set; } = string.Empty;
    public InspectionType Type { get; set; }

    // Vehicle info
    public int? VehicleYear { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleBodyClass { get; set; }

    // Damage markers
    public List<DamageMarkerDto> DamageMarkers { get; set; } = new();

    public string? Notes { get; set; }
    public bool HasSignature { get; set; }

    // Location
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Timestamps
    public DateTime InspectedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Inspector info
    public Guid InspectedById { get; set; }
    public string? InspectorName { get; set; }

    // Photos
    public List<DocumentDto> Photos { get; set; } = new();
}

public class DamageMarkerDto
{
    public double X { get; set; }  // 0.0 - 1.0 position on vehicle diagram
    public double Y { get; set; }  // 0.0 - 1.0 position on vehicle diagram
    public string? Description { get; set; }
    public string? Severity { get; set; }  // Minor, Moderate, Severe
}
