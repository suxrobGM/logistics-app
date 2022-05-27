namespace Logistics.Domain.Specifications;

public class TenantsSpecification : BaseSpecification<Tenant>
{
    public TenantsSpecification(string searchInput)
        : base(i =>
            (!string.IsNullOrEmpty(i.Name) &&
            i.Name.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) ||

            (!string.IsNullOrEmpty(i.DisplayName) &&
            i.DisplayName.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase))
        )
    {
    }
}
