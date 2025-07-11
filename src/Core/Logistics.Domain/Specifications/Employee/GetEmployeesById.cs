using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetEmployeesById : BaseSpecification<Employee>
{
    public GetEmployeesById(Guid[] userIds)
    {
        Criteria = i => userIds.Contains(i.Id);
        ApplyOrderBy("Id");
    }
}
