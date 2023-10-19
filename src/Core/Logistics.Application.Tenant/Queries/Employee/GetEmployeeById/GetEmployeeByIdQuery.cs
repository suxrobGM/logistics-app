using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetEmployeeByIdQuery : IRequest<ResponseResult<EmployeeDto>>
{
    public string? UserId { get; set; }
}
