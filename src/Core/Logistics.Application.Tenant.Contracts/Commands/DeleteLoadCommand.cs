namespace Logistics.Application.Contracts.Commands;

public sealed class DeleteLoadCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
}
