using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetPayrolls : BaseSpecification<Payroll>
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

    protected override Expression<Func<Payroll, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "paymentamount" => i => i.Payment.Amount,
            "paymentdate" => i => i.Payment.PaymentDate,
            "paymentmethod" => i => i.Payment.Method,
            "paymentstatus" => i => i.Payment.Status,
            "employeefirstname" => i => i.Employee.FirstName,
            "employeelastname" => i => i.Employee.LastName,
            "employeeemail" => i => i.Employee.Email,
            "employeesalary" => i => i.Employee.Salary,
            "employeesalarytype" => i => i.Employee.SalaryType,
            _ => i => i.Payment.Status
        };
    }
}
