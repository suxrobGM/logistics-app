using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetPayrollInvoicesQuery : SearchableQuery, IRequest<PagedResult<InvoiceDto>>
{
    /// <summary>
    /// Filter payrolls by Employee ID
    /// </summary>
    public string? EmployeeId { get; set; }
}
