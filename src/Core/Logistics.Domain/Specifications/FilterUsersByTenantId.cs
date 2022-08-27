namespace Logistics.Domain.Specifications;

public class FilterUsersByTenantId : BaseSpecification<User>
{
    public FilterUsersByTenantId(string tenantId)
    {
        Criteria = i => i.JoinedTenantIds.Contains(tenantId);
    }
}