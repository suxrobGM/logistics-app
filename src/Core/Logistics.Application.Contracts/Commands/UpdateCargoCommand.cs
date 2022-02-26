namespace Logistics.Application.Contracts.Commands;

public class UpdateCargoCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public decimal PricePerMile { get; set; }
    public double TotalTripMiles { get; set; }
    public string? AssignedTruckId { get; set; }
    public string? Status { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? PickUpDate { get; set; }
}
