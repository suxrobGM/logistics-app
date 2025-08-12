using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class FilterInvoicesByEmployeeName : BaseSpecification<PayrollInvoice>
{
    public FilterInvoicesByEmployeeName(
        string employeeName,
        string? orderBy,
        int page,
        int pageSize)
    {
        Criteria = i =>
            !string.IsNullOrEmpty(i.Employee.FirstName) && i.Employee.FirstName.Contains(employeeName) ||
            !string.IsNullOrEmpty(i.Employee.LastName) && i.Employee.LastName.Contains(employeeName);

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }

    // protected override Expression<Func<PayrollInvoice, object?>> CreateOrderByExpression(string propertyName)
    // {
    //     return propertyName switch
    //     {
    //         "total" => i => i.Total,
    //         "createdat" => i => i.CreatedAt,
    //         "number" => i => i.Number,
    //         "status" => i => i.Status,
    //         "periodstart" => i => i.PeriodStart,
    //         "periodend" => i => i.PeriodEnd,
    //         "employee.firstname" => i => i.Employee.FirstName,
    //         "employee.lastname" => i => i.Employee.LastName,
    //         "employee.email" => i => i.Employee.Email,
    //         "employee.salary" => i => i.Employee.Salary,
    //         "employee.salarytype" => i => i.Employee.SalaryType,
    //         _ => i => i.Status
    //     };
    // }
}
