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

public record CreateEldDriverMappingDto
{
    public Guid EmployeeId { get; set; }
    public EldProviderType ProviderType { get; set; }
    public required string ExternalDriverId { get; set; }
    public string? ExternalDriverName { get; set; }
}

public record CreateEldVehicleMappingDto
{
    public Guid TruckId { get; set; }
    public EldProviderType ProviderType { get; set; }
    public required string ExternalVehicleId { get; set; }
    public string? ExternalVehicleName { get; set; }
}

/// <summary>
/// Represents an external driver from an ELD provider that hasn't been mapped yet
/// </summary>
public record UnmappedEldDriverDto
{
    public required string ExternalDriverId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public EldProviderType ProviderType { get; set; }
}

/// <summary>
/// Represents an external vehicle from an ELD provider that hasn't been mapped yet
/// </summary>
public record UnmappedEldVehicleDto
{
    public required string ExternalVehicleId { get; set; }
    public string? Name { get; set; }
    public string? Vin { get; set; }
    public string? LicensePlate { get; set; }
    public EldProviderType ProviderType { get; set; }
}
