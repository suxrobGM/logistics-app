using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class GetEmployeeLoads : BaseSpecification<Load>
{
    public GetEmployeeLoads(Guid userId)
    {
        Criteria = i => i.AssignedDispatcherId == userId; // || i..AssignedDriverId == userId;
    }
}
