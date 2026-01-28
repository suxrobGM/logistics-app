using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class UpdateTruckCommand : IAppRequest
{
    public Guid Id { get; set; }
    public string? TruckNumber { get; set; }
    public TruckType? TruckType { get; set; }
    public TruckStatus? TruckStatus { get; set; }
    public Guid? MainDriverId { get; set; }
    public Guid? SecondaryDriverId { get; set; }
    public int? VehicleCapacity { get; set; }
}
