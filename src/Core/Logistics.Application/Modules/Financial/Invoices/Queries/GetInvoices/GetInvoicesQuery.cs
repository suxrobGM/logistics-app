using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

/// <summary><see cref="SearchableQuery.Search"/> matches the invoice number, customer name or employee name.</summary>
public class GetInvoicesQuery : SearchableQuery, IQuery<PagedResult<InvoiceDto>>
{
    public InvoiceType? Type { get; set; }
    public InvoiceStatus[]? Statuses { get; set; }
    public Guid? LoadId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? EmployeeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? OverdueOnly { get; set; }
    public SalaryType[]? SalaryTypes { get; set; }
    public FactoringStatus? FactoringStatus { get; set; }
}
