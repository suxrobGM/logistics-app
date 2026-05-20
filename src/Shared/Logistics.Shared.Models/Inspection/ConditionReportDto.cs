using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Shared.Models;

public class ConditionReportDto
{
    public Guid Id { get; set; }
    public Guid LoadId { get; set; }
    public string? LoadReferenceId { get; set; }

    /// <summary>
    /// The load's cargo type. Drives which identifier section the clients render
    /// (VIN block for Vehicle, container block for container types, neither otherwise)
    /// and which part catalog is valid for defects.
    /// </summary>
    public LoadType LoadType { get; set; }

    public InspectionType Type { get; set; }

    // Vehicle-cargo identifier (only populated when LoadType == Vehicle)
    public string? Vin { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleBodyClass { get; set; }

    // Container-cargo identifier (only populated for container LoadTypes)
    public string? ContainerNumber { get; set; }
    public string? SealNumber { get; set; }

    public List<ConditionDefectDto> Defects { get; set; } = [];

    public string? Notes { get; set; }
    public bool HasSignature { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime InspectedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid InspectedById { get; set; }
    public string? InspectorName { get; set; }

    public List<DocumentDto> Photos { get; set; } = [];
}

public class ConditionDefectDto
{
    public Guid Id { get; set; }
    public CargoInspectionPartCategory PartCategory { get; set; }
    public string PartCategoryDisplay { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; }
    public string SeverityDisplay { get; set; } = string.Empty;
}

/// <summary>
/// Per-load-type catalog of valid <see cref="CargoInspectionPartCategory"/> values
/// returned by <c>GET /inspections/parts</c>.
/// </summary>
public class InspectionPartCatalogDto
{
    public LoadType LoadType { get; set; }
    public List<InspectionPartCategoryDto> Categories { get; set; } = [];
}

public class InspectionPartCategoryDto
{
    public CargoInspectionPartCategory Value { get; set; }
    public string Display { get; set; } = string.Empty;
}
