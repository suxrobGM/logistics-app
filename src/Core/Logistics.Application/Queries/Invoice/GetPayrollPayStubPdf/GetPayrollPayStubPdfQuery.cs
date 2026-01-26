using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetPayrollPayStubPdfQuery : IAppRequest<Result<InvoicePdfResult>>
{
    public Guid PayrollInvoiceId { get; set; }
}
