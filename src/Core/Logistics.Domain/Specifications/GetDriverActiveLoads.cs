namespace Logistics.Domain.Specifications;

public class GetDriverActiveLoads : BaseSpecification<Load>
{
    public GetDriverActiveLoads(string userId)
    {
        Criteria = i =>
            i.DeliveryDate.HasValue &&
            i.AssignedTruck != null &&
            i.AssignedTruck.Drivers.Select(emp => emp.Id).Contains(userId);
    }
}
