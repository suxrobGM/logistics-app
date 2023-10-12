namespace Logistics.Application.Tenant.Commands;

public class DeleteEmployeeCommand : Request<ResponseResult>
{
    public string? UserId { get; set; }
}
