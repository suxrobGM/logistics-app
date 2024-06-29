using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetEmployeeByIdQuery : IRequest<Result<EmployeeDto>>
{
    public string UserId { get; set; } = default!;
}
