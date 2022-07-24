namespace Logistics.Application.Contracts.Commands;

public sealed class UpdateLoadCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDriverId { get; set; }
    public string? AssignedTruckId { get; set; }
    public string? Status { get; set; }
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
}
