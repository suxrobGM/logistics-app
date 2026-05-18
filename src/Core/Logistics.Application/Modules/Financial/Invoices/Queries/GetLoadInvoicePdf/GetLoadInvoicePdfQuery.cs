using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

public class GetLoadInvoicePdfQuery : IQuery<Result<InvoicePdfResult>>
{
    public Guid InvoiceId { get; set; }
}

public record InvoicePdfResult(byte[] PdfBytes, string FileName);
