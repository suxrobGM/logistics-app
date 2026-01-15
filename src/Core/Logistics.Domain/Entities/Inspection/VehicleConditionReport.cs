using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class VehicleConditionReport : AuditableEntity, ITenantEntity
{
    public required Guid LoadId { get; set; }
    public virtual Load Load { get; set; } = null!;

    public required string Vin { get; set; }
    public required InspectionType Type { get; set; }

    // Vehicle info from VIN decode
    public int? VehicleYear { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleBodyClass { get; set; }

    // Damage markers as JSON array: [{x: 0.25, y: 0.5, description: "Scratch on door"}]
    public string? DamageMarkersJson { get; set; }

    public string? Notes { get; set; }
    public string? InspectorSignature { get; set; }  // Base64 encoded or blob path

    // Location where inspection was performed
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime InspectedAt { get; set; }
    public required Guid InspectedById { get; set; }
    public virtual Employee InspectedBy { get; set; } = null!;

    // Navigation property for photos
    public virtual ICollection<LoadDocument> Photos { get; set; } = new List<LoadDocument>();

    public static VehicleConditionReport Create(
        Guid loadId,
        string vin,
        InspectionType type,
        Guid inspectedById,
        string? damageMarkersJson = null,
        string? notes = null,
        double? latitude = null,
        double? longitude = null)
    {
        return new VehicleConditionReport
        {
            LoadId = loadId,
            Vin = vin,
            Type = type,
            InspectedById = inspectedById,
            DamageMarkersJson = damageMarkersJson,
            Notes = notes,
            Latitude = latitude,
            Longitude = longitude,
            InspectedAt = DateTime.UtcNow
        };
    }
}
