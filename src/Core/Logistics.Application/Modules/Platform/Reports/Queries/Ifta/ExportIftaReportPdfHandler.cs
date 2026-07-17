using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Documents;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Reports.Queries;

internal sealed class ExportIftaReportPdfHandler(
    IIftaReportService iftaReportService,
    IIftaReportPdfService pdfService,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<ExportIftaReportPdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(ExportIftaReportPdfQuery req, CancellationToken ct)
    {
        var report = await iftaReportService.GetReportAsync(req.Year, req.Quarter, ct);
        var pdf = pdfService.GenerateIftaReportPdf(report, tenantUow.GetCurrentTenant());
        return Result<byte[]>.Ok(pdf);
    }
}
