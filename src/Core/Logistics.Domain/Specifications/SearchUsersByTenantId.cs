namespace Logistics.Domain.Specifications;

public class SearchUsersByTenantId : BaseSpecification<User>
{
    public SearchUsersByTenantId(string search, string tenantId)
        : base(i =>
            i.JoinedTenantIds.Contains(tenantId) && 
            (
                (!string.IsNullOrEmpty(i.FirstName) &&
                 i.FirstName.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||
                
                (!string.IsNullOrEmpty(i.LastName) &&
                 i.LastName.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||
                
                (!string.IsNullOrEmpty(i.UserName) &&
                 i.UserName.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||
                
                (!string.IsNullOrEmpty(i.Email) &&
                 i.Email.Contains(search, StringComparison.InvariantCultureIgnoreCase))
            )
        )
    {
    }
}