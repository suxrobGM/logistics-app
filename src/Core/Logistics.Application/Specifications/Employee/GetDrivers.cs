using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class GetDrivers : BaseSpecification<Employee>
{
    public GetDrivers()
    {
        Criteria = i => i.TruckId.HasValue;
    }
}
