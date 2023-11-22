using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchTenants : BaseSpecification<Tenant>
{
    public SearchTenants(
        string? search, 
        string? orderBy, 
        bool descending)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i =>
                (!string.IsNullOrEmpty(i.Name) &&
                 i.Name.Contains(search)) ||

                (!string.IsNullOrEmpty(i.CompanyName) &&
                 i.CompanyName.Contains(search));
        }
        
        ApplyOrderBy(InitOrderBy(orderBy), descending);
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
