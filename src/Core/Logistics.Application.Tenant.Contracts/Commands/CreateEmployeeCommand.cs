namespace Logistics.Application.Tenant.Commands;

public sealed class CreateEmployeeCommand : RequestBase<ResponseResult>
{
    public string? Id { get; set; }
    public string? Role { get; set; }
}
