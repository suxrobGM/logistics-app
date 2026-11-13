using Logistics.Application.Modules.Reporting.Reports.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class TrucksProfitabilityHandler(IProfitabilityService profitability)
    : IRequestHandler<TrucksProfitabilityQuery, Result<List<TruckProfitDto>>>
{
    public async Task<Result<List<TruckProfitDto>>> Handle(TrucksProfitabilityQuery req, CancellationToken ct)
    {
        var result = await profitability.BuildAsync(req.StartDate, req.EndDate, ct);
        var trucks = result.Trucks.Where(t => req.TruckId is null || t.TruckId == req.TruckId).ToList();
        return Result<List<TruckProfitDto>>.Ok(trucks);
    }
}
