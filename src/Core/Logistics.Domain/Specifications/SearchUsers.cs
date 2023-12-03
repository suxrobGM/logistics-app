using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchUsers : BaseSpecification<User>
{
    public SearchUsers(
        string? search, 
        string? orderBy,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i =>
                i.FirstName.Contains(search) ||
                i.LastName.Contains(search) ||
                (i.PhoneNumber != null && i.PhoneNumber.Contains(search)) ||
                (i.Email != null && i.Email.Contains(search));
        }
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<User, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "firstname" => i => i.FirstName,
            "lastname" => i => i.LastName,
            "phonenumber" => i => i.PhoneNumber,
            _ => i => i.Email
        };
    }
}
