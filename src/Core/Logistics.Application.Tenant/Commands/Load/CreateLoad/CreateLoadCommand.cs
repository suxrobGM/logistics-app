using Logistics.Domain.ValueObjects;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class CreateLoadCommand : IRequest<Result>
{
    public string? Name { get; set; }
    public Address? OriginAddress { get; set; }
    public double OriginAddressLat { get; set; }
    public double OriginAddressLong { get; set; }
    public Address? DestinationAddress { get; set; }
    public double DestinationAddressLat { get; set; }
    public double DestinationAddressLong { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public string AssignedDispatcherId { get; set; } = default!;
    public string AssignedTruckId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
}
