namespace Logistics.Domain.Specifications;

public class SearchAppRoles : BaseSpecification<AppRole>
{
    public SearchAppRoles(string? search)
    {
        if (string.IsNullOrEmpty(search))
            return;
        
        Criteria = i =>
            (i.Name != null && i.Name.Contains(search)) ||
            (i.DisplayName != null && i.DisplayName.Contains(search));
    }
}
