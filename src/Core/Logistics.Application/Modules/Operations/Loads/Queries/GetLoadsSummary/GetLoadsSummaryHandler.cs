using Logistics.Application.Abstractions.Context;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadsSummaryHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetLoadsSummaryQuery, Result<LoadsSummaryDto>>
{
    public async Task<Result<LoadsSummaryDto>> Handle(GetLoadsSummaryQuery req, CancellationToken ct)
    {
        var isDriver = currentUserService.IsTenantDriver();
        var driverId = isDriver ? currentUserService.GetUserId() : req.DriverId;

        if (isDriver && driverId is null)
        {
            return Result<LoadsSummaryDto>.Ok(new LoadsSummaryDto());
        }

        var groups = await tenantUow.Repository<Load>().Query()
            .FilterForList(req, driverId)
            .GroupBy(l => l.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Revenue = g.Sum(l => l.DeliveryCost) })
            .ToListAsync(ct);

        return Result<LoadsSummaryDto>.Ok(new LoadsSummaryDto
        {
            TotalCount = groups.Sum(g => g.Count),
            TotalRevenue = groups.Sum(g => g.Revenue),
            StatusCounts = groups
                .OrderBy(g => g.Status)
                .Select(g => new LoadStatusCountDto { Status = g.Status, Count = g.Count })
                .ToList()
        });
    }
}
