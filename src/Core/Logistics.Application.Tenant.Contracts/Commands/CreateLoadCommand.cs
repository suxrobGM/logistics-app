namespace Logistics.Application.Contracts.Commands;

public sealed class CreateLoadCommand : RequestBase<DataResult>
{
    public string? Name { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDriverId { get; set; }
}
