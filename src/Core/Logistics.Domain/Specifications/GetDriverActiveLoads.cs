using Logistics.Domain.Enums;

namespace Logistics.Domain.Specifications;

public class GetDriverActiveLoads : BaseSpecification<Load>
{
    public GetDriverActiveLoads(string userId)
    {
        Criteria = i =>
            i.Status != LoadStatus.Delivered &&
            i.AssignedTruck != null &&
            i.AssignedTruck.Drivers.Select(emp => emp.Id).Contains(userId);
    }
}
