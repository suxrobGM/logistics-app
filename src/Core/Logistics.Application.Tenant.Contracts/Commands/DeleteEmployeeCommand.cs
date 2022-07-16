namespace Logistics.Application.Contracts.Commands;

public sealed class DeleteEmployeeCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
}
