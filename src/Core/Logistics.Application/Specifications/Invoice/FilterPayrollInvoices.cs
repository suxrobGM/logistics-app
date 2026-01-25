using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

/// <summary>
/// Specification for filtering payroll invoices with comprehensive filter options.
/// </summary>
public sealed class FilterPayrollInvoices : BaseSpecification<PayrollInvoice>
{
    public FilterPayrollInvoices(
        Guid? employeeId,
        string? employeeName,
        InvoiceStatus? status,
        SalaryType? salaryType,
        DateTime? startDate,
        DateTime? endDate,
        string? orderBy,
        int page,
        int pageSize)
    {
        Expression<Func<PayrollInvoice, bool>>? criteria = null;

        // Filter by Employee ID
        if (employeeId.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.EmployeeId == employeeId.Value);
        }

        // Filter by Employee Name (partial match)
        if (!string.IsNullOrWhiteSpace(employeeName))
        {
            var lowerName = employeeName.ToLower();
            criteria = AddCriteria(criteria, i =>
                i.Employee.FirstName.ToLower().Contains(lowerName) ||
                i.Employee.LastName.ToLower().Contains(lowerName));
        }

        // Filter by Status
        if (status.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.Status == status.Value);
        }

        // Filter by Salary Type
        if (salaryType.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.Employee.SalaryType == salaryType.Value);
        }

        // Filter by date range (based on PeriodStart/PeriodEnd)
        if (startDate.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.PeriodEnd >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.PeriodStart <= endDate.Value);
        }

        if (criteria is not null)
        {
            Criteria = criteria;
        }

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }

    private static Expression<Func<PayrollInvoice, bool>> AddCriteria(
        Expression<Func<PayrollInvoice, bool>>? existing,
        Expression<Func<PayrollInvoice, bool>> newCriteria)
    {
        if (existing is null)
        {
            return newCriteria;
        }

        // Combine expressions with AND
        var parameter = Expression.Parameter(typeof(PayrollInvoice), "i");
        var combined = Expression.AndAlso(
            Expression.Invoke(existing, parameter),
            Expression.Invoke(newCriteria, parameter));

        return Expression.Lambda<Func<PayrollInvoice, bool>>(combined, parameter);
    }
}
