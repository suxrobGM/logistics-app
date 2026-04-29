using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Commands;

public class UpdateLoadCommand : IAppRequest
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public LoadType? Type { get; set; }
    public Address? OriginAddress { get; set; }
    public GeoPoint? OriginLocation { get; set; }
    public Address? DestinationAddress { get; set; }
    public GeoPoint? DestinationLocation { get; set; }
    public decimal? DeliveryCost { get; set; }
    public double? Distance { get; set; }
    public Guid? AssignedDispatcherId { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public Guid? CustomerId { get; set; }
    public LoadStatus? Status { get; set; }

    public LoadSource? Source { get; set; }
    public DateTime? RequestedPickupDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public Guid? ContainerId { get; set; }
    public Guid? OriginTerminalId { get; set; }
    public Guid? DestinationTerminalId { get; set; }
}
