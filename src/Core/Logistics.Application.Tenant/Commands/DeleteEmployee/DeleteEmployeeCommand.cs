namespace Logistics.Application.Tenant.Commands;

public class DeleteEmployeeCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
}
