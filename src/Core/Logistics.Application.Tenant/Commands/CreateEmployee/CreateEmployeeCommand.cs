namespace Logistics.Application.Tenant.Commands;

public class CreateEmployeeCommand : Request<ResponseResult>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}
