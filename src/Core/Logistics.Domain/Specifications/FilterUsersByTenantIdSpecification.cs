namespace Logistics.Domain.Specifications;

public class FilterUsersByTenantIdSpecification : BaseSpecification<User>
{
    public FilterUsersByTenantIdSpecification(string tenantId)
        : base(i => i.JoinedTenantIds.Contains(tenantId))
    {
    }
}