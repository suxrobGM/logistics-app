using Logistics.Application.Modules.Reporting.Reports.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class ProfitabilityReportHandler(IProfitabilityService profitability)
    : IRequestHandler<ProfitabilityReportQuery, Result<ProfitabilityReportDto>>
{
    public async Task<Result<ProfitabilityReportDto>> Handle(ProfitabilityReportQuery req, CancellationToken ct) =>
        Result<ProfitabilityReportDto>.Ok(await profitability.BuildAsync(req.FromDate, req.ToDate, ct));
}
