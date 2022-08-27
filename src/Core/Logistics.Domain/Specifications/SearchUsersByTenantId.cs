namespace Logistics.Domain.Specifications;

public class SearchUsersByTenantId : BaseSpecification<User>
{
    public SearchUsersByTenantId(string? search, string tenantId, string? orderBy = "JoinedDate")
    {
        OrderBy = i => orderBy == "UserName" ? i.UserName : i.JoinedDate;
        
        if (string.IsNullOrEmpty(search))
            return;
        
        Criteria = i =>
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
            );
    }
}