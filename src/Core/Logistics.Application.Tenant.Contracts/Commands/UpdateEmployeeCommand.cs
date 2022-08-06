namespace Logistics.Application.Contracts.Commands;

public sealed class UpdateEmployeeCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
    public string? Role { get; set; }
}
