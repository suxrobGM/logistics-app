namespace Logistics.Application.Tenant.Commands;

public class UpdateEmployeeCommand : Request<ResponseResult>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}
