using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchCustomers : BaseSpecification<Customer>
{
    public SearchCustomers(
        string? search, 
        string? orderBy,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i => i.Name.Contains(search);
        }
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<Customer, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            // "firstname" => i => i.FirstName!,
            // "lastname" => i => i.LastName!,
            // "phonenumber" => i => i.PhoneNumber!,
            _ => i => i.Name
        };
    }
}
