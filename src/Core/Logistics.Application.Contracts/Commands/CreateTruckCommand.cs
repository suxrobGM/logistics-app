namespace Logistics.Application.Contracts.Commands;

public sealed class CreateTruckCommand : RequestBase<DataResult>
{
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }
}
