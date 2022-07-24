namespace Logistics.Domain.Specifications;

public class FilterUsersByTenantId : BaseSpecification<User>
{
    public FilterUsersByTenantId(string tenantId)
        : base(i => i.JoinedTenantIds.Contains(tenantId))
    {
    }
}