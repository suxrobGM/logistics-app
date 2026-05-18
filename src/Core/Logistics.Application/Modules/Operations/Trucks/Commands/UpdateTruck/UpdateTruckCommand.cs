using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

public class UpdateTruckCommand : ICommand
{
    public Guid Id { get; set; }
    public string? TruckNumber { get; set; }
    public TruckType? TruckType { get; set; }
    public TruckStatus? TruckStatus { get; set; }
    public Guid? MainDriverId { get; set; }
    public Guid? SecondaryDriverId { get; set; }
    public int? VehicleCapacity { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? Vin { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicensePlateState { get; set; }

    /// <summary>
    /// Replaces the truck's ADR equipment configuration when supplied. Null leaves it unchanged.
    /// </summary>
    public AdrEquipmentDto? AdrEquipment { get; set; }

    /// <summary>
    /// US Hazmat placarding flag. Null leaves it unchanged.
    /// </summary>
    public bool? IsHazmatPlacarded { get; set; }
}
