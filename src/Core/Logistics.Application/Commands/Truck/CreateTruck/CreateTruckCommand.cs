using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class CreateTruckCommand : IAppRequest
{
    public string TruckNumber { get; set; } = null!;
    public TruckType TruckType { get; set; }
    public Guid MainDriverId { get; set; }
    public int? VehicleCapacity { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? Vin { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicensePlateState { get; set; }
}
