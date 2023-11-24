using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchEmployees : BaseSpecification<Employee>
{
    public SearchEmployees(
        string? search, 
        string? orderBy,
        int page,
        int pageSize,
        bool descending)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i =>
                (i.FirstName != null && i.FirstName.Contains(search)) ||
                (i.LastName != null && i.LastName.Contains(search)) ||
                (i.PhoneNumber != null && i.PhoneNumber.Contains(search)) ||
                (i.Email != null && i.Email.Contains(search));
        }
        
        ApplyPaging(page, pageSize);
        ApplyOrderBy(InitOrderBy(orderBy), descending);
    }
    
    private static Expression<Func<Employee, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? "email";
        return propertyName switch
        {
            "firstname" => i => i.FirstName,
            "lastname" => i => i.LastName,
            "phonenumber" => i => i.PhoneNumber,
            "salary" => i => i.Salary,
            "salarytype" => i => i.SalaryType,
            _ => i => i.Email
        };
    }
}
