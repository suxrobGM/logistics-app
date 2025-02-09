using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetEmployeesQuery : SearchableQuery, IRequest<PagedResult<EmployeeDto>>
{
    public string? Role { get; set; }
}
