using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class PreviewPayrollInvoiceQuery : IAppRequest<Result<InvoiceDto>>
{
    public Guid EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
