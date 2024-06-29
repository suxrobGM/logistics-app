using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetDrivers : BaseSpecification<Employee>
{
    public GetDrivers()
    {
        Criteria = i => !string.IsNullOrEmpty(i.TruckId);
    }
}
