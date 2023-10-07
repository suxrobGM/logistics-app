using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetDriverActiveLoads : BaseSpecification<Load>
{
    public GetDriverActiveLoads(string userId)
    {
        Criteria = i =>
            i.DeliveryDate == null &&
            i.AssignedTruck != null &&
            i.AssignedTruck.Drivers.Select(emp => emp.Id).Contains(userId);
    }
}
