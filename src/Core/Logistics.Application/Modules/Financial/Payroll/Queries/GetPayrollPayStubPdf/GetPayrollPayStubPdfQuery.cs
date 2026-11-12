using Logistics.Shared.Models;
using Logistics.Application.Modules.Financial.Invoices.Queries;

namespace Logistics.Application.Modules.Financial.Payroll.Queries;

public class GetPayrollPayStubPdfQuery : IQuery<Result<InvoicePdfResult>>
{
    public Guid PayrollInvoiceId { get; set; }
}
