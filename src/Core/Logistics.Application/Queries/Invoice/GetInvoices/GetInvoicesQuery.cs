using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetInvoicesQuery : PagedQuery, IRequest<PagedResult<InvoiceDto>>
{
    /// <summary>
    /// Filter invoices by Load ID
    /// </summary>
    public Guid? LoadId { get; set; }
    
    /// <summary>
    /// Filter invoices by Employee ID
    /// </summary>
    public Guid? EmployeeId { get; set; }
    
    /// <summary>
    /// Filter invoices by Employee Name
    /// </summary>
    public string? EmployeeName { get; set; }
    
    /// <summary>
    /// Filter invoices by type, if you don't specify, all type of invoices will be returned
    /// </summary>
    public InvoiceType? InvoiceType { get; set; }
}
