using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetEmployeesQuery : SearchableRequest<EmployeeDto>
{
    public string? Role { get; set; }
}
