using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchAppRoles : BaseSpecification<AppRole>
{
    public SearchAppRoles(
        string? search,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i =>
                (i.Name != null && i.Name.Contains(search)) ||
                (i.DisplayName != null && i.DisplayName.Contains(search));
        }
        
        ApplyPaging(page, pageSize);
    }
}
