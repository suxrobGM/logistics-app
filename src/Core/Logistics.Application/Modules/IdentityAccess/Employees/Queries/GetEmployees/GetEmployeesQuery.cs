using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Queries;

public class GetEmployeesQuery : SearchableQuery, IQuery<PagedResult<EmployeeDto>>
{
    /// <summary>
    /// Exact tenant role names to include (e.g. <c>tenant.driver</c>); empty returns every role.
    /// A list because "who can drive" spans Driver and Owner - an owner-operator drives their own.
    /// </summary>
    public string[] Roles { get; set; } = [];
}
