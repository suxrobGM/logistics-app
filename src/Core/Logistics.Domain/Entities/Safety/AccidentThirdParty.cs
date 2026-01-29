using Logistics.Domain.Core;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Third party (other vehicle/person) involved in an accident
/// </summary>
public class AccidentThirdParty : Entity, ITenantEntity
{
    public Guid AccidentReportId { get; set; }
    public virtual AccidentReport AccidentReport { get; set; } = null!;

    public required string Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? DriverLicense { get; set; }

    // Vehicle info
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehicleLicensePlate { get; set; }
    public string? VehicleVin { get; set; }
    public string? VehicleColor { get; set; }

    // Insurance info
    public string? InsuranceCompany { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceAgentPhone { get; set; }
}
