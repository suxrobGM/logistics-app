using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetEmployeeByIdQuery : Request<ResponseResult<EmployeeDto>>
{
    public string? UserId { get; set; }
}
