using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadsHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetLoadsQuery, PagedResult<LoadDto>>
{
    public async Task<PagedResult<LoadDto>> Handle(
        GetLoadsQuery req,
        CancellationToken ct)
    {
        var isDriver = currentUserService.IsTenantDriver();
        var userId = isDriver ? currentUserService.GetUserId() : req.UserId;

        if (isDriver && userId is null)
        {
            return PagedResult<LoadDto>.Ok([], 0, req.PageSize);
        }

        var baseQuery = tenantUow.Repository<Load>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            baseQuery = baseQuery.Where(i =>
                (i.Name != null && i.Name.ToLower().Contains(search)) ||
                (i.Customer != null && i.Customer.Name.ToLower().Contains(search)) ||
                i.Number.ToString().Contains(search) ||
                i.OriginAddress.Line1.ToLower().Contains(search) ||
                (i.OriginAddress.Line2 != null && i.OriginAddress.Line2.ToLower().Contains(search)) ||
                i.DestinationAddress.Line1.ToLower().Contains(search) ||
                (i.DestinationAddress.Line2 != null && i.DestinationAddress.Line2.ToLower().Contains(search)));
        }

        if (req.OnlyActiveLoads)
        {
            baseQuery = baseQuery.Where(i =>
                i.Status == LoadStatus.Dispatched || i.Status == LoadStatus.PickedUp);
        }

        if (userId.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.AssignedTruck != null &&
                                             (i.AssignedTruck.MainDriverId == userId ||
                                              i.AssignedTruck.SecondaryDriverId == userId));
        }

        if (req.TruckId.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.AssignedTruckId == req.TruckId);
        }

        if (req.Statuses?.Length > 0)
        {
            baseQuery = baseQuery.Where(i => req.Statuses.Contains(i.Status));
        }

        if (req.Types?.Length > 0)
        {
            baseQuery = baseQuery.Where(i => req.Types.Contains(i.Type));
        }

        if (req.CustomerId.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.CustomerId == req.CustomerId.Value);
        }

        if (req is { StartDate: not null, EndDate: not null })
        {
            baseQuery = baseQuery.Where(i => i.DispatchedAt >= req.StartDate &&
                                             i.DispatchedAt <= req.EndDate);
        }

        var totalItems = await baseQuery.CountAsync(ct);

        baseQuery = baseQuery.OrderBy(req.OrderBy);

        if (!req.LoadAllPages)
        {
            baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);
        }

        var entities = await baseQuery.ToArrayAsync(ct);
        var lookup = await LoadLookupResolver.ResolveAsync(tenantUow, entities, ct);
        var loads = entities.Select(i => i.ToDto(lookup)).ToArray();

        return PagedResult<LoadDto>.Ok(loads, totalItems, req.PageSize);
    }
}
