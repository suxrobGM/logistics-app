using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetEmployeesById : BaseSpecification<Employee>
{
    public GetEmployeesById(string[] userIds)
    {
        OrderBy = i => i.Id;
        Criteria = i => userIds.Contains(i.Id);
    }
}
