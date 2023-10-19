using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetEmployeesQuery : SearchableQuery, IRequest<PagedResponseResult<EmployeeDto>>
{
    public string? Role { get; set; }
}
