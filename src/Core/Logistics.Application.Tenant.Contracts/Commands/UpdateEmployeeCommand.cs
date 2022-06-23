namespace Logistics.Application.Contracts.Commands;

public sealed class UpdateEmployeeCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? UserName { get; init; }
}
