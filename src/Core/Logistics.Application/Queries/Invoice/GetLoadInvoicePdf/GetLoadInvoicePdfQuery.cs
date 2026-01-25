using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetLoadInvoicePdfQuery : IAppRequest<Result<InvoicePdfResult>>
{
    public Guid InvoiceId { get; set; }
}

public record InvoicePdfResult(byte[] PdfBytes, string FileName);
