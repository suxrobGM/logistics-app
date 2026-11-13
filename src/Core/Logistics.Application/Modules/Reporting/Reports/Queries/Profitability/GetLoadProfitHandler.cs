using Logistics.Application.Modules.Reporting.Reports.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class GetLoadProfitHandler(ITenantUnitOfWork tenantUow, IProfitabilityService profitability)
    : IRequestHandler<GetLoadProfitQuery, Result<LoadProfitDto>>
{
    public async Task<Result<LoadProfitDto>> Handle(GetLoadProfitQuery req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().Query()
            .Where(l => l.Id == req.LoadId)
            .Select(l => new { l.DeliveredAt })
            .FirstOrDefaultAsync(ct);

        if (load is null)
        {
            return Result<LoadProfitDto>.Fail($"Could not find a load with ID '{req.LoadId}'", ErrorCodes.NotFound);
        }

        if (load.DeliveredAt is not { } delivered)
        {
            return Result<LoadProfitDto>.Fail("Profit is available once the load is delivered.");
        }

        var monthStart = new DateTime(delivered.Year, delivered.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = await profitability.BuildAsync(monthStart, monthStart.AddMonths(1), ct);
        var row = result.Loads.First(l => l.LoadId == req.LoadId);
        return Result<LoadProfitDto>.Ok(row);
    }
}
