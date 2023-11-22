using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchUsers : BaseSpecification<User>
{
    public SearchUsers(
        string? search, 
        string? orderBy, 
        bool descending)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i =>
                i.FirstName.Contains(search) ||
                i.LastName.Contains(search) ||
                (i.PhoneNumber != null && i.PhoneNumber.Contains(search)) ||
                (i.Email != null && i.Email.Contains(search));
        }
        
        ApplyOrderBy(InitOrderBy(orderBy), descending);
    }
    
    private static Expression<Func<User, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? "email";
        return propertyName switch
        {
            "firstname" => i => i.FirstName,
            "lastname" => i => i.LastName,
            "phonenumber" => i => i.PhoneNumber,
            _ => i => i.Email
        };
    }
}
