namespace Logistics.Application.Contracts.Commands;

public class UpdateTruckCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
    public string? DriverId { get; set; }
}
