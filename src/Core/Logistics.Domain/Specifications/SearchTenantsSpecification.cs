namespace Logistics.Domain.Specifications;

public class SearchTenantsSpecification : BaseSpecification<Tenant>
{
    public SearchTenantsSpecification(string search)
        : base(i =>
            (!string.IsNullOrEmpty(i.Name) &&
            i.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||

            (!string.IsNullOrEmpty(i.DisplayName) &&
            i.DisplayName.Contains(search, StringComparison.InvariantCultureIgnoreCase))
        )
    {
    }
}
