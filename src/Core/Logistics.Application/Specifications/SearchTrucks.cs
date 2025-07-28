using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public sealed class SearchTrucks : BaseSpecification<Truck>
{
    public SearchTrucks(
        string? search,
        string? orderBy,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i => i.Number.Contains(search) ||
                            i.MainDriver != null && (i.MainDriver.FirstName.Contains(search) ||
                                                     i.MainDriver.LastName.Contains(search)) ||
                            (i.SecondaryDriver != null &&
                             (i.SecondaryDriver.FirstName.Contains(search) ||
                              i.SecondaryDriver.LastName.Contains(search)));
        }
        
        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
}
