namespace Logistics.Application.Contracts.Commands;

public sealed class CreateEmployeeCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
    public string? TenantId { get; set; }
    public string? Role { get; set; }
}
