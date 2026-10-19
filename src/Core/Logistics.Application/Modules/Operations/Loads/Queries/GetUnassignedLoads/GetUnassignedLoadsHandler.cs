using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

/// <summary>
/// Handler for getting loads that are not assigned to any active trip.
/// </summary>
internal sealed class GetUnassignedLoadsHandler(ITenantUnitOfWork uow, ILoadLookupBuilder lookupBuilder)
    : IRequestHandler<GetUnassignedLoadsQuery, PagedResult<LoadDto>>
{
    public async Task<PagedResult<LoadDto>> Handle(GetUnassignedLoadsQuery req, CancellationToken ct)
    {
        var loadRepo = uow.Repository<Load>();

        // Get loads that:
        // 1. Are in Draft status (ready to be assigned)
        // 2. Are not already part of any trip (no trip stops)
        var baseQuery = loadRepo.Query()
            .Where(l => l.Status == LoadStatus.Draft && l.TripStops.Count == 0);

        if (req.LoadIds is { Length: > 0 } ids)
            baseQuery = baseQuery.Where(l => ids.Contains(l.Id));

        var totalItems = await loadRepo.CountAsync(
            predicate: l => l.Status == LoadStatus.Draft && l.TripStops.Count == 0,
            ct: ct);

        var entities = await baseQuery
            .OrderBy(req.OrderBy, l => l.Id, defaultOrderBy: "-CreatedAt")
            .ApplyPaging(req.Page, req.PageSize)
            .ToArrayAsync(ct);

        var lookup = await lookupBuilder.BuildAsync(entities, ct);
        var loads = entities.Select(l => l.ToDto(lookup)).ToArray();

        return PagedResult<LoadDto>.Ok(loads, totalItems, req.PageSize);
    }
}
