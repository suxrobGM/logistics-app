using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadsHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService,
    ILoadLookupBuilder lookupBuilder)
    : IRequestHandler<GetLoadsQuery, PagedResult<LoadDto>>
{
    private static readonly SortFields<Load> Sorts = new SortFields<Load>("-createdAt")
        .Add("number", x => x.Number)
        .Add("name", x => x.Name)
        .Add("type", x => x.Type)
        .Add("customer.name", x => x.Customer!.Name)
        .Add("originAddress", x => x.OriginAddress.City)
        .Add("status", x => x.Status)
        .Add("distance", x => x.Distance)
        .Add("deliveryCost", x => x.DeliveryCost)
        .Add("assignedTruckNumber", x => x.AssignedTruck!.Number)
        .Add("assignedDispatcherName", x => x.AssignedDispatcher!.FirstName)
        .Add("dispatchedAt", x => x.DispatchedAt)
        .Add("deliveredAt", x => x.DeliveredAt)
        .Add("createdAt", x => x.CreatedAt);

    public async Task<PagedResult<LoadDto>> Handle(
        GetLoadsQuery req,
        CancellationToken ct)
    {
        var isDriver = currentUserService.IsTenantDriver();
        var userId = isDriver ? currentUserService.GetUserId() : req.DriverId;

        if (isDriver && userId is null)
        {
            return PagedResult<LoadDto>.Ok([], 0, req.PageSize);
        }

        var baseQuery = tenantUow.Repository<Load>().Query().FilterForList(req, userId);

        var totalItems = await baseQuery.CountAsync(ct);

        var entities = await Sorts.Apply(baseQuery, req.OrderBy, x => x.Id)
            .ApplyPaging(req.Page, req.PageSize)
            .ToArrayAsync(ct);

        var lookup = await lookupBuilder.BuildAsync(entities, ct);
        var loads = entities.Select(i => i.ToDto(lookup)).ToArray();

        return PagedResult<LoadDto>.Ok(loads, totalItems, req.PageSize);
    }
}
