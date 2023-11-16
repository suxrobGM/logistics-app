using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchTenants : BaseSpecification<Tenant>
{
    public SearchTenants(
        string? search, 
        string? orderBy = "Name", 
        bool descending = false)
    {
        Descending = descending;
        OrderBy = InitOrderBy(orderBy);
        
        if (string.IsNullOrEmpty(search))
            return;
        
        Criteria = i =>
            (!string.IsNullOrEmpty(i.Name) &&
             i.Name.Contains(search)) ||

            (!string.IsNullOrEmpty(i.CompanyName) &&
             i.CompanyName.Contains(search));
    }
    
    private static Expression<Func<Tenant, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? "name";
        return propertyName switch
        {
            "displayname" => i => i.CompanyName,
            _ => i => i.Name
        };
    }
}
