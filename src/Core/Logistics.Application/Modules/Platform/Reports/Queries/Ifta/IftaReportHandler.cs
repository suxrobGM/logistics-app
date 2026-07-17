using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Reports.Queries;

internal sealed class IftaReportHandler(IIftaReportService iftaReportService)
    : IAppRequestHandler<IftaReportQuery, Result<IftaReportDto>>
{
    public async Task<Result<IftaReportDto>> Handle(IftaReportQuery req, CancellationToken ct)
    {
        var report = await iftaReportService.GetReportAsync(req.Year, req.Quarter, ct);
        return Result<IftaReportDto>.Ok(report);
    }
}
