using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Detailed accident/incident report
/// </summary>
public class AccidentReport : AuditableEntity, ITenantEntity
{
    public Guid DriverId { get; set; }
    public virtual Employee Driver { get; set; } = null!;

    public Guid TruckId { get; set; }
    public virtual Truck Truck { get; set; } = null!;

    public Guid? TripId { get; set; }
    public virtual Trip? Trip { get; set; }

    public required AccidentReportStatus Status { get; set; }
    public required AccidentType AccidentType { get; set; }
    public required AccidentSeverity Severity { get; set; }

    // Incident details
    public required DateTime AccidentDateTime { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? WeatherConditions { get; set; }
    public string? RoadConditions { get; set; }

    // Injuries
    public bool AnyInjuries { get; set; }
    public int? NumberOfInjuries { get; set; }
    public string? InjuryDescription { get; set; }

    // Damage
    public bool VehicleDamaged { get; set; }
    public string? VehicleDamageDescription { get; set; }
    public decimal? EstimatedDamageCost { get; set; }
    public bool VehicleDrivable { get; set; } = true;

    // Police report
    public bool PoliceReportFiled { get; set; }
    public string? PoliceReportNumber { get; set; }
    public string? PoliceOfficerName { get; set; }
    public string? PoliceOfficerBadge { get; set; }
    public string? PoliceDepartment { get; set; }

    // Insurance
    public bool InsuranceNotified { get; set; }
    public DateTime? InsuranceNotifiedAt { get; set; }
    public string? InsuranceClaimNumber { get; set; }

    // Driver statement
    public string? DriverStatement { get; set; }
    public string? DriverSignature { get; set; }
    public DateTime? DriverSignedAt { get; set; }

    // Review
    public Guid? ReviewedById { get; set; }
    public virtual Employee? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public virtual List<AccidentWitness> Witnesses { get; set; } = [];
    public virtual List<AccidentThirdParty> ThirdParties { get; set; } = [];
    public virtual List<TruckDocument> Documents { get; set; } = [];
}
