using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetPayrolls : BaseSpecification<PayrollInvoice>
{
    public GetPayrolls(
        string? search,
        string? orderBy,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i =>
                !string.IsNullOrEmpty(i.Employee.FirstName) && i.Employee.FirstName.Contains(search) || 
                !string.IsNullOrEmpty(i.Employee.LastName) && i.Employee.LastName.Contains(search);
        }

        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }

    protected override Expression<Func<PayrollInvoice, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "total" => i => i.Total,
            "createdat" => i => i.CreatedAt,
            "invoicenumber" => i => i.Number,
            "status" => i => i.Status,
            "periodstart" => i => i.PeriodStart,
            "periodend" => i => i.PeriodEnd,
            "employeefirstname" => i => i.Employee.FirstName,
            "employeelastname" => i => i.Employee.LastName,
            "employeeemail" => i => i.Employee.Email,
            "employeesalary" => i => i.Employee.Salary,
            "employeesalarytype" => i => i.Employee.SalaryType,
            _ => i => i.Status
        };
    }
}
