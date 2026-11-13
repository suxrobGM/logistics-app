using Logistics.Application.Modules.Reporting.Reports.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class ProfitabilitySummaryHandler(IProfitabilityService profitability)
    : IRequestHandler<ProfitabilitySummaryQuery, Result<ProfitabilitySummaryDto>>
{
    public async Task<Result<ProfitabilitySummaryDto>> Handle(ProfitabilitySummaryQuery req, CancellationToken ct)
    {
        var result = await profitability.BuildAsync(req.StartDate, req.EndDate, ct);
        return Result<ProfitabilitySummaryDto>.Ok(result.Summary);
    }
}
