using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Commands;

public class CreateLoadCommand : IAppRequest
{
    public string Name { get; set; } = null!;
    public LoadType Type { get; set; }
    public Address OriginAddress { get; set; } = null!;
    public GeoPoint OriginLocation { get; set; } = null!;
    public Address DestinationAddress { get; set; } = null!;
    public GeoPoint DestinationLocation { get; set; } = null!;
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public Guid AssignedDispatcherId { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public Guid CustomerId { get; set; }

    public LoadSource Source { get; set; } = LoadSource.Manual;
    public DateTime? RequestedPickupDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public Guid? ContainerId { get; set; }
    public Guid? OriginTerminalId { get; set; }
    public Guid? DestinationTerminalId { get; set; }
}
