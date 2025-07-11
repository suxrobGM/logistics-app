using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class FilterEmployeesByRole : BaseSpecification<Employee>
{
    public FilterEmployeesByRole(Guid roleId)
    {
        Criteria = e => e.EmployeeRoles.Any(er => er.RoleId == roleId);
    }
}
