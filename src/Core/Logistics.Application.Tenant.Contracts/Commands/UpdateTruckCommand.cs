namespace Logistics.Application.Contracts.Commands;

public sealed class UpdateTruckCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
    public string? DriverId { get; set; }
}
