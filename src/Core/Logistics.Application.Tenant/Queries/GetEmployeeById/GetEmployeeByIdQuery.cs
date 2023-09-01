using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetEmployeeByIdQuery : Request<ResponseResult<EmployeeDto>>
{
    public string? Id { get; set; }
}
