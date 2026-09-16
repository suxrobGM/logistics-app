using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

public class GetPayrollPayStubPdfQuery : IQuery<Result<InvoicePdfResult>>
{
    public Guid PayrollInvoiceId { get; set; }
}
