using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetInvoicesQuery : SearchableQuery, IAppRequest<PagedResult<InvoiceDto>>
{
    /// <summary>
    ///     Filter invoices by Load ID
    /// </summary>
    public Guid? LoadId { get; set; }

    /// <summary>
    ///     Filter invoices by Employee ID
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    ///     Filter invoices by Employee Name
    /// </summary>
    public string? EmployeeName { get; set; }

    /// <summary>
    ///     Filter invoices by type, if you don't specify, all type of invoices will be returned
    /// </summary>
    public InvoiceType? InvoiceType { get; set; }

    /// <summary>
    ///     Filter invoices by status
    /// </summary>
    public InvoiceStatus? Status { get; set; }

    /// <summary>
    ///     Filter invoices by Customer ID
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    ///     Filter invoices by Customer Name (partial match)
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    ///     Filter invoices created after this date
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    ///     Filter invoices created before this date
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    ///     Filter to show only overdue invoices
    /// </summary>
    public bool? OverdueOnly { get; set; }

    /// <summary>
    ///     Filter payroll invoices by employee salary type
    /// </summary>
    public SalaryType? SalaryType { get; set; }
}
