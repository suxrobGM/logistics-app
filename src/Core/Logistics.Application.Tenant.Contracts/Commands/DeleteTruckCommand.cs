namespace Logistics.Application.Contracts.Commands;

public sealed class DeleteTruckCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
}
