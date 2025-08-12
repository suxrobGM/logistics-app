using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class PreviewPayrollInvoiceQuery : IRequest<Result<InvoiceDto>>
{
    public Guid EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
