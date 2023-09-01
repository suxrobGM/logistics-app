namespace Logistics.Application.Tenant.Commands;

public class UpdateEmployeeCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
    public string? Role { get; set; }
}
