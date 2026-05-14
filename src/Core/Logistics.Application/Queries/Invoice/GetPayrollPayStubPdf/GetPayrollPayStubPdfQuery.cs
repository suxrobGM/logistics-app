using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetPayrollPayStubPdfQuery : IQuery<Result<InvoicePdfResult>>
{
    public Guid PayrollInvoiceId { get; set; }
}
