using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class SearchTenantRoles : BaseSpecification<TenantRole>
{
    public SearchTenantRoles(string? search, int page, int pageSize)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i =>
                i.Name.Contains(search) ||
                (i.DisplayName != null && i.DisplayName.Contains(search));
        }

        ApplyPaging(page, pageSize);
    }
}
