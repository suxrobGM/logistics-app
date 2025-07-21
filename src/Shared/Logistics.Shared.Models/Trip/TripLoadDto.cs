using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record TripLoadDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public string? Name { get; set; }
    public LoadStatus Status { get; set; }
    public double Distance { get; set; }
    public decimal DeliveryCost { get; set; }
    public required AddressDto OriginAddress { get; set; }
    public required AddressDto DestinationAddress { get; set; }
    public required CustomerDto Customer { get; set; }
}
