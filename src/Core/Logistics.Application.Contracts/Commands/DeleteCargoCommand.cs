namespace Logistics.Application.Contracts.Commands;

public sealed class DeleteCargoCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
}
