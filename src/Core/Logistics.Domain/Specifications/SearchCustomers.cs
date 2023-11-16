using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchCustomers : BaseSpecification<Customer>
{
    public SearchCustomers(string? search, 
        string? orderBy = "Name", 
        bool descending = false)
    {
        Descending = descending;
        OrderBy = InitOrderBy(orderBy);
        
        if (string.IsNullOrEmpty(search))
            return;
        
        Criteria = i => i.Name.Contains(search);
    }
    
    private static Expression<Func<Customer, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? "name";
        return propertyName switch
        {
            // "firstname" => i => i.FirstName!,
            // "lastname" => i => i.LastName!,
            // "phonenumber" => i => i.PhoneNumber!,
            _ => i => i.Name
        };
    }
}
