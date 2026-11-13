using Logistics.Application.Common.Extensions;
using Logistics.Application.Modules.Reporting.Reports.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class LoadsProfitabilityHandler(IProfitabilityService profitability)
    : IRequestHandler<LoadsProfitabilityQuery, PagedResult<LoadProfitDto>>
{
    public async Task<PagedResult<LoadProfitDto>> Handle(LoadsProfitabilityQuery req, CancellationToken ct)
    {
        var result = await profitability.BuildAsync(req.StartDate, req.EndDate, ct);
        var loads = result.Loads.AsQueryable();

        if (req.TruckId is { } truckId)
        {
            loads = loads.Where(l => l.TruckId == truckId);
        }

        if (req.CustomerId is { } customerId)
        {
            loads = loads.Where(l => l.CustomerId == customerId);
        }

        if (!string.IsNullOrWhiteSpace(req.Lane))
        {
            loads = loads.Where(l => l.Lane == req.Lane);
        }

        var totalItems = loads.Count();
        var page = loads
            .OrderBy(req.OrderBy, l => l.LoadId, nameof(LoadProfitDto.Margin))
            .ApplyPaging(req.Page, req.PageSize)
            .ToArray();
        return PagedResult<LoadProfitDto>.Ok(page, totalItems, req.PageSize);
    }
}
