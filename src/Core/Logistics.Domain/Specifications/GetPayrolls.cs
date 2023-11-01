using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetPayrolls : BaseSpecification<Payroll>
{
    public GetPayrolls(
        string? orderBy, 
        bool descending = false)
    {
        Descending = descending;
        OrderBy = InitOrderBy(orderBy);
    }

    private static Expression<Func<Payroll, object>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? string.Empty;
        return propertyName switch
        {
            "paymentamount" => i => i.Payment.Amount,
            "paymentdate" => i => i.Payment.PaymentDate!,
            "paymentmethod" => i => i.Payment.Method,
            "employeefirstname" => i => i.Employee.FirstName!,
            "employeelastname" => i => i.Employee.LastName!,
            "employeeemail" => i => i.Employee.Email!,
            "employeesalary" => i => i.Employee.Salary,
            "employeesalarytype" => i => i.Employee.SalaryType,
            _ => i => i.Id
        };
    }
}
