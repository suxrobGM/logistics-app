namespace Logistics.Domain.Specifications;

public class SearchTenantRoles : BaseSpecification<TenantRole>
{
    public SearchTenantRoles(string search)
    {
        Criteria = i =>
            (!string.IsNullOrEmpty(i.Name) &&
             i.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||

            (!string.IsNullOrEmpty(i.DisplayName) &&
             i.DisplayName.Contains(search, StringComparison.InvariantCultureIgnoreCase));
    }
}