using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Queries;

public class GetEmployeesQuery : SearchableQuery, IQuery<PagedResult<EmployeeDto>>
{
    public string? Role { get; set; }
}
