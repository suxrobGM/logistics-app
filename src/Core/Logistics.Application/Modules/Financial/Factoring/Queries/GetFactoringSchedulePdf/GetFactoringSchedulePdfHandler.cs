using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Documents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Queries;

internal sealed class GetFactoringSchedulePdfHandler(
    ITenantUnitOfWork tenantUow,
    IFactoringSchedulePdfService pdfService)
    : IRequestHandler<GetFactoringSchedulePdfQuery, Result<InvoicePdfResult>>
{
    public async Task<Result<InvoicePdfResult>> Handle(GetFactoringSchedulePdfQuery req, CancellationToken ct)
    {
        var batch = await tenantUow.Repository<FactoringBatch>().GetByIdAsync(req.BatchId, ct);
        if (batch is null)
        {
            return Result<InvoicePdfResult>.Fail("Schedule of accounts not found.");
        }

        var pdf = pdfService.GenerateSchedulePdf(batch, tenantUow.GetCurrentTenant());
        return Result<InvoicePdfResult>.Ok(new InvoicePdfResult(pdf, $"Schedule-{batch.Number}.pdf"));
    }
}
