using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

internal sealed class GetTripsSummaryHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTripsSummaryQuery, Result<TripsSummaryDto>>
{
    public async Task<Result<TripsSummaryDto>> Handle(GetTripsSummaryQuery req, CancellationToken ct)
    {
        var groups = await tenantUow.Repository<Trip>().Query()
            .FilterForList(req)
            .GroupBy(t => t.Status)
            .Select(g => new TripStatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return Result<TripsSummaryDto>.Ok(new TripsSummaryDto
        {
            TotalCount = groups.Sum(g => g.Count),
            StatusCounts = groups.OrderBy(g => g.Status).ToList()
        });
    }
}
