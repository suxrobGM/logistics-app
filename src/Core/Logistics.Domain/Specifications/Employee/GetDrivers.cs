using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetDrivers : BaseSpecification<Employee>
{
    public GetDrivers()
    {
        Criteria = i => i.TruckId.HasValue;
    }
}
