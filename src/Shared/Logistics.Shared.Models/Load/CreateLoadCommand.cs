namespace Logistics.Shared.Models;

public record CreateLoadCommand
{
    public string? Name { get; set; }
    public string? SourceAddress { get; set; }

    public string? DestinationAddress { get; set; }

    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDriverId { get; set; }
}
