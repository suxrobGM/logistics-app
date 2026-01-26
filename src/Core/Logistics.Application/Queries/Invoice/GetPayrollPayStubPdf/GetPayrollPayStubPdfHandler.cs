using Logistics.Application.Abstractions;
using Logistics.Application.Services.Pdf;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPayrollPayStubPdfHandler(ITenantUnitOfWork tenantUow, IPayrollPayStubService pdfService)
    : IAppRequestHandler<GetPayrollPayStubPdfQuery, Result<InvoicePdfResult>>
{
    public async Task<Result<InvoicePdfResult>> Handle(GetPayrollPayStubPdfQuery request, CancellationToken ct)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetByIdAsync(request.PayrollInvoiceId, ct);

        if (payroll is null)
        {
            return Result<InvoicePdfResult>.Fail($"Could not find a payroll invoice with ID {request.PayrollInvoiceId}");
        }

        var tenant = tenantUow.GetCurrentTenant();
        var pdfBytes = pdfService.GeneratePayStubPdf(payroll, tenant);
        var fileName = $"PayStub-{payroll.Number}.pdf";

        return Result<InvoicePdfResult>.Ok(new InvoicePdfResult(pdfBytes, fileName));
    }
}
