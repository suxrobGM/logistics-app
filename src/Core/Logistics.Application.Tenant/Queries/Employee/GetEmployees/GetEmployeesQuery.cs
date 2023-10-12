using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetEmployeesQuery : SearchableQuery<EmployeeDto>
{
    public string? Role { get; set; }
}
