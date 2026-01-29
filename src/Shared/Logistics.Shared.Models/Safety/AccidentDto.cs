using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Shared.Models;

public class AccidentReportDto
{
    public Guid Id { get; set; }
    public Guid TruckId { get; set; }
    public string TruckNumber { get; set; } = string.Empty;
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;

    public AccidentType Type { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public AccidentSeverity Severity { get; set; }
    public string SeverityDisplay { get; set; } = string.Empty;
    public AccidentReportStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;

    public DateTime AccidentDateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? WeatherConditions { get; set; }
    public string? RoadConditions { get; set; }

    public bool PoliceReportFiled { get; set; }
    public string? PoliceReportNumber { get; set; }
    public string? PoliceOfficerName { get; set; }
    public string? PoliceDepartment { get; set; }

    public bool InjuriesReported { get; set; }
    public int? NumberOfInjuries { get; set; }
    public string? InjuryDescription { get; set; }
    public bool FatalitiesReported { get; set; }
    public int? NumberOfFatalities { get; set; }

    public bool VehicleTowed { get; set; }
    public string? TowCompany { get; set; }
    public decimal? EstimatedDamage { get; set; }
    public string? DamageDescription { get; set; }

    public bool HazmatInvolved { get; set; }
    public string? HazmatDescription { get; set; }

    public string? InsuranceClaimNumber { get; set; }
    public DateTime? InsuranceNotifiedAt { get; set; }

    public Guid? ReviewedById { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public Guid? TripId { get; set; }

    public List<AccidentWitnessDto> Witnesses { get; set; } = [];
    public List<AccidentThirdPartyDto> ThirdParties { get; set; } = [];
    public List<DocumentDto> Photos { get; set; } = [];
    public List<DocumentDto> Documents { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}

public class AccidentWitnessDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Statement { get; set; }
    public DateTime? StatementDate { get; set; }
}

public class AccidentThirdPartyDto
{
    public Guid Id { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public string? DriverLicense { get; set; }
    public string? DriverLicenseState { get; set; }

    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleYear { get; set; }
    public string? VehicleColor { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicensePlateState { get; set; }
    public string? VinNumber { get; set; }

    public string? InsuranceCompany { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsurancePhone { get; set; }

    public string? Notes { get; set; }
}

public class AccidentSummaryDto
{
    public int TotalAccidents { get; set; }
    public int PendingReview { get; set; }
    public int ThisMonth { get; set; }
    public int ThisYear { get; set; }
    public int WithInjuries { get; set; }
    public decimal TotalEstimatedDamage { get; set; }
}

#region Request DTOs

public class CreateAccidentReportRequest
{
    public required Guid TruckId { get; set; }
    public required Guid DriverId { get; set; }
    public required AccidentType Type { get; set; }
    public required AccidentSeverity Severity { get; set; }
    public required DateTime AccidentDateTime { get; set; }
    public required string Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public required string Description { get; set; }
    public string? WeatherConditions { get; set; }
    public string? RoadConditions { get; set; }
    public bool InjuriesReported { get; set; }
    public int? NumberOfInjuries { get; set; }
    public string? InjuryDescription { get; set; }
    public bool FatalitiesReported { get; set; }
    public int? NumberOfFatalities { get; set; }
    public bool VehicleTowed { get; set; }
    public string? TowCompany { get; set; }
    public decimal? EstimatedDamage { get; set; }
    public string? DamageDescription { get; set; }
    public bool HazmatInvolved { get; set; }
    public string? HazmatDescription { get; set; }
    public Guid? TripId { get; set; }
}

public class AddAccidentWitnessRequest
{
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Statement { get; set; }
}

public class AddAccidentThirdPartyRequest
{
    public required string DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? DriverLicense { get; set; }
    public string? DriverLicenseState { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleYear { get; set; }
    public string? VehicleColor { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicensePlateState { get; set; }
    public string? VinNumber { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsurancePhone { get; set; }
    public string? Notes { get; set; }
}

#endregion
