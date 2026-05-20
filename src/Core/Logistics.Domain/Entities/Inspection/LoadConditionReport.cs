using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Cargo condition report captured at pickup or delivery for a load. Covers
/// vehicle cargo (auto-haul), intermodal containers, and generic freight —
/// VIN-related fields are populated only when the load's <see cref="LoadType"/>
/// is <see cref="LoadType.Vehicle"/>; <see cref="ContainerNumber"/> and
/// <see cref="SealNumber"/> are populated only for container loads. Defects
/// are stored in the <see cref="Defects"/> child collection.
/// </summary>
public class LoadConditionReport : AuditableEntity, ITenantEntity
{
    public required Guid LoadId { get; set; }
    public virtual Load Load { get; set; } = null!;

    public required InspectionType Type { get; set; }

    // Vehicle-cargo identifier (only for LoadType.Vehicle)
    public string? Vin { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleBodyClass { get; set; }

    // Container-cargo identifier (only for container LoadTypes)
    public string? ContainerNumber { get; set; }
    public string? SealNumber { get; set; }

    public string? Notes { get; set; }
    public string? InspectorSignature { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime InspectedAt { get; set; }
    public required Guid InspectedById { get; set; }
    public virtual Employee InspectedBy { get; set; } = null!;

    /// <summary>
    /// Defects documented during inspection. Each defect's
    /// <see cref="ConditionDefect.PartCategory"/> must be valid for the load's type.
    /// </summary>
    public virtual List<ConditionDefect> Defects { get; set; } = [];

    /// <summary>
    /// Photos captured at inspection time. Stored as LoadDocument rows linked
    /// to the load and discriminated by capture timestamp + document type.
    /// </summary>
    public virtual ICollection<LoadDocument> Photos { get; set; } = new List<LoadDocument>();

    public static LoadConditionReport Create(
        Guid loadId,
        InspectionType type,
        Guid inspectedById,
        string? notes = null,
        double? latitude = null,
        double? longitude = null)
    {
        return new LoadConditionReport
        {
            LoadId = loadId,
            Type = type,
            InspectedById = inspectedById,
            Notes = notes,
            Latitude = latitude,
            Longitude = longitude,
            InspectedAt = DateTime.UtcNow
        };
    }
}
