using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

internal sealed class GetTrucksSummaryHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTrucksSummaryQuery, Result<TrucksSummaryDto>>
{
    public async Task<Result<TrucksSummaryDto>> Handle(GetTrucksSummaryQuery req, CancellationToken ct)
    {
        var groups = await tenantUow.Repository<Truck>().Query()
            .FilterForList(req)
            .GroupBy(t => t.Status)
            .Select(g => new TruckStatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return Result<TrucksSummaryDto>.Ok(new TrucksSummaryDto
        {
            TotalCount = groups.Sum(g => g.Count),
            StatusCounts = groups.OrderBy(g => g.Status).ToList()
        });
    }
}
