using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record EldDriverMappingDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public EldProviderType ProviderType { get; set; }
    public string? ExternalDriverId { get; set; }
    public string? ExternalDriverName { get; set; }
    public bool IsSyncEnabled { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

public record EldVehicleMappingDto
{
    public Guid Id { get; set; }
    public Guid TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public EldProviderType ProviderType { get; set; }
    public string? ExternalVehicleId { get; set; }
    public string? ExternalVehicleName { get; set; }
    public bool IsSyncEnabled { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

/// <summary>Which field an automatic ELD-to-TMS match agreed on, strongest first.</summary>
public enum EldMatchReason
{
    Email,
    LicenseNumber,
    Name,
    Vin,
    TruckNumber
}

public record EldDriverMappingSuggestionDto
{
    public required string ExternalDriverId { get; set; }
    public string? ExternalDriverName { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public EldMatchReason MatchedBy { get; set; }
}

public record EldVehicleMappingSuggestionDto
{
    public required string ExternalVehicleId { get; set; }
    public string? ExternalVehicleName { get; set; }
    public Guid TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public EldMatchReason MatchedBy { get; set; }
}

/// <summary>Unambiguous matches between the provider's unmapped drivers/vehicles and the tenant's.</summary>
public record EldMappingSuggestionsDto
{
    public List<EldDriverMappingSuggestionDto> Drivers { get; set; } = [];
    public List<EldVehicleMappingSuggestionDto> Vehicles { get; set; } = [];
}
