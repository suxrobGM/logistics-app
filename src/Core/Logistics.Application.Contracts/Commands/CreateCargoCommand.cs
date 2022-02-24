namespace Logistics.Application.Contracts.Commands;

public class CreateCargoCommand : RequestBase<DataResult>
{
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public decimal PricePerMile { get; set; }
    public double TotalTripMiles { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedTruckId { get; set; }
}
