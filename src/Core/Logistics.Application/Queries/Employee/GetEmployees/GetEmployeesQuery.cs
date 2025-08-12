using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetEmployeesQuery : SearchableQuery, IAppRequest<PagedResult<EmployeeDto>>
{
    public string? Role { get; set; }
}
