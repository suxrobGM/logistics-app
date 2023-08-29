namespace Logistics.Application.Tenant.Commands;

public sealed class DeleteEmployeeCommand : RequestBase<ResponseResult>
{
    public string? Id { get; set; }
}
