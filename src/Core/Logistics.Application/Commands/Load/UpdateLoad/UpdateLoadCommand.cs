using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Logistics.Domain.Primitives.Enums;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateLoadCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public LoadType? Type { get; set; }
    public Address? OriginAddress { get; set; }
    public double? OriginAddressLat { get; set; }
    public double? OriginAddressLong { get; set; }
    public Address? DestinationAddress { get; set; }
    public double? DestinationAddressLat { get; set; }
    public double? DestinationAddressLong { get; set; }
    public decimal? DeliveryCost { get; set; }
    public double? Distance { get; set; }
    public Guid? AssignedDispatcherId { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public Guid? CustomerId { get; set; }
    public LoadStatus? Status { get; set; }
}
