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
}
