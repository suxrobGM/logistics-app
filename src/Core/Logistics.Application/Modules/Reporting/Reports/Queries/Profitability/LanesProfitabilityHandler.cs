using Logistics.Application.Modules.Reporting.Reports.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class LanesProfitabilityHandler(IProfitabilityService profitability)
    : IRequestHandler<LanesProfitabilityQuery, Result<List<LaneProfitDto>>>
{
    public async Task<Result<List<LaneProfitDto>>> Handle(LanesProfitabilityQuery req, CancellationToken ct)
    {
        var result = await profitability.BuildAsync(req.StartDate, req.EndDate, ct);
        return Result<List<LaneProfitDto>>.Ok(result.Lanes.ToList());
    }
}
