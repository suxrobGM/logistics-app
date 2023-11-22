using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class FilterEmployeesByRole : BaseSpecification<Employee>
{
    public FilterEmployeesByRole(string roleId)
    {
        Criteria = e => e.EmployeeRoles.Any(er => er.RoleId == roleId);
    }
}
