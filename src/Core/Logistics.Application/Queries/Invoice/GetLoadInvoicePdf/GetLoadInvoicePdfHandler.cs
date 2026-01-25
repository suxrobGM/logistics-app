using Logistics.Application.Abstractions;
using Logistics.Application.Services.Pdf;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetLoadInvoicePdfHandler(ITenantUnitOfWork tenantUow, IInvoicePdfService pdfService)
    : IAppRequestHandler<GetLoadInvoicePdfQuery, Result<InvoicePdfResult>>
{
    public async Task<Result<InvoicePdfResult>> Handle(GetLoadInvoicePdfQuery request, CancellationToken ct)
    {
        var invoice = await tenantUow.Repository<LoadInvoice>().GetByIdAsync(request.InvoiceId, ct);

        if (invoice is null)
        {
            return Result<InvoicePdfResult>.Fail($"Could not find an invoice with ID {request.InvoiceId}");
        }

        var tenant = tenantUow.GetCurrentTenant();
        var pdfBytes = pdfService.GenerateLoadInvoicePdf(invoice, tenant);
        var fileName = $"Invoice-{invoice.Number}.pdf";

        return Result<InvoicePdfResult>.Ok(new InvoicePdfResult(pdfBytes, fileName));
    }
}
