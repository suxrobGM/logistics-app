using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class GetEmployeesById : BaseSpecification<Employee>
{
    public GetEmployeesById(Guid[] userIds)
    {
        Criteria = i => userIds.Contains(i.Id);
        OrderBy("Id");
    }
}
