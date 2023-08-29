namespace Logistics.Application.Tenant.Commands;

public class RemoveEmployeeRoleCommand : RequestBase<ResponseResult>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}