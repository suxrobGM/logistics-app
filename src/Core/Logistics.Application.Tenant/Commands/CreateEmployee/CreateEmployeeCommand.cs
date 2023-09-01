namespace Logistics.Application.Tenant.Commands;

public class CreateEmployeeCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
    public string? Role { get; set; }
}
