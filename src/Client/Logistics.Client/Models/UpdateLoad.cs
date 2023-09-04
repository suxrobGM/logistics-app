using Logistics.Models;

namespace Logistics.Client.Models;

public record UpdateLoad
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public double? DeliveryCost { get; set; }
    public double? Distance { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDriverId { get; set; }
    public LoadStatusDto? Status { get; set; }
}
