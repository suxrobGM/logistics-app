namespace Logistics.Application.Contracts.Commands;

public sealed class UpdateLoadCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public decimal DeliveryCost { get; set; }
    public double TotalTripMiles { get; set; }
    public string? AssignedTruckId { get; set; }
    public string? Status { get; set; }
    public DateTime PickUpDate { get; set; }
}
