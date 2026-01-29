using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Driver Vehicle Inspection Report - FMCSA required pre/post-trip inspections
/// </summary>
public class DvirReport : AuditableEntity, ITenantEntity
{
    public Guid TruckId { get; set; }
    public virtual Truck Truck { get; set; } = null!;

    public Guid DriverId { get; set; }
    public virtual Employee Driver { get; set; } = null!;

    /// <summary>
    /// Pre-trip or Post-trip inspection
    /// </summary>
    public required DvirType Type { get; set; }

    /// <summary>
    /// Current status of the DVIR
    /// </summary>
    public DvirStatus Status { get; set; } = DvirStatus.Draft;

    /// <summary>
    /// When the inspection was performed
    /// </summary>
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Latitude of inspection location
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude of inspection location
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Odometer reading at time of inspection
    /// </summary>
    public int? OdometerReading { get; set; }

    /// <summary>
    /// Whether any defects were found during inspection
    /// </summary>
    public bool HasDefects { get; set; }

    /// <summary>
    /// Driver's signature (Base64 encoded image)
    /// </summary>
    public string? DriverSignature { get; set; }

    /// <summary>
    /// Driver's notes or comments
    /// </summary>
    public string? DriverNotes { get; set; }

    /// <summary>
    /// Mechanic/reviewer who signed off on the report
    /// </summary>
    public Guid? ReviewedById { get; set; }
    public virtual Employee? ReviewedBy { get; set; }

    /// <summary>
    /// When the report was reviewed by mechanic
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Mechanic's signature (Base64 encoded image)
    /// </summary>
    public string? MechanicSignature { get; set; }

    /// <summary>
    /// Mechanic's notes about repairs or condition
    /// </summary>
    public string? MechanicNotes { get; set; }

    /// <summary>
    /// Whether all reported defects have been corrected
    /// </summary>
    public bool? DefectsCorrected { get; set; }

    /// <summary>
    /// Associated trip (optional)
    /// </summary>
    public Guid? TripId { get; set; }
    public virtual Trip? Trip { get; set; }

    /// <summary>
    /// List of defects found during inspection
    /// </summary>
    public virtual List<DvirDefect> Defects { get; set; } = [];

    /// <summary>
    /// Photos taken during inspection
    /// </summary>
    public virtual List<TruckDocument> Photos { get; set; } = [];
}
