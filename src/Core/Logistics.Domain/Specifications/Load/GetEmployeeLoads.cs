using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetEmployeeLoads : BaseSpecification<Load>
{
    public GetEmployeeLoads(Guid userId)
    {
        Criteria = i => i.AssignedDispatcherId == userId; // || i..AssignedDriverId == userId;
    }
}
