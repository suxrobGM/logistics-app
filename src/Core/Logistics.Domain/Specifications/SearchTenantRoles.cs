namespace Logistics.Domain.Specifications;

public class SearchTenantRoles : BaseSpecification<TenantRole>
{
    public SearchTenantRoles(string? search)
    {
        if (string.IsNullOrEmpty(search))
            return;
        
        Criteria = i =>
            i.Name.Contains(search) ||
            (i.DisplayName != null && i.DisplayName.Contains(search));
    }
}
