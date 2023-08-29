namespace Logistics.Application.Tenant.Commands;

public sealed class UpdateEmployeeCommand : RequestBase<ResponseResult>
{
    public string? Id { get; set; }
    public string? Role { get; set; }
}
