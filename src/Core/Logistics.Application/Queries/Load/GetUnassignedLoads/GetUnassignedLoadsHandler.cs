using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Handler for getting loads that are not assigned to any active trip.
/// </summary>
internal sealed class GetUnassignedLoadsHandler(ITenantUnitOfWork uow)
    : IAppRequestHandler<GetUnassignedLoadsQuery, PagedResult<LoadDto>>
{
    public async Task<PagedResult<LoadDto>> Handle(GetUnassignedLoadsQuery req, CancellationToken ct)
    {
        var loadRepo = uow.Repository<Load>();

        // Get loads that:
        // 1. Are in Draft status (ready to be assigned)
        // 2. Are not already part of any trip (no trip stops)
        var baseQuery = loadRepo.Query()
            .Where(l => l.Status == LoadStatus.Draft && l.TripStops.Count == 0);

        var totalItems = await loadRepo.CountAsync(
            predicate: l => l.Status == LoadStatus.Draft && l.TripStops.Count == 0,
            ct: ct);

        var loads = baseQuery
            .ApplyPaging(req.Page, req.PageSize)
            .Select(l => l.ToDto())
            .ToArray();

        return PagedResult<LoadDto>.Succeed(loads, totalItems, req.PageSize);
    }
}
