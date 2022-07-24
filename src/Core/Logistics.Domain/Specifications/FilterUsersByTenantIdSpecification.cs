namespace Logistics.Domain.Specifications;

public class FilterUsersByTenantIdSpecification : BaseSpecification<User>
{
    public FilterUsersByTenantIdSpecification(string tenantId)
        : base(i => i.JoinedTenants.Contains(tenantId))
    {
    }
}