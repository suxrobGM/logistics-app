using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTripsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTripsQuery, PagedResult<TripDto>>
{
    public async Task<PagedResult<TripDto>> Handle(
        GetTripsQuery req, CancellationToken ct)
    {
        var totalItems = await tenantUow.Repository<Trip>().CountAsync(ct: ct);
        var spec = new GetTripsSpec(
            req.Name,
            req.Status,
            req.TruckNumber,
            req.TruckId,
            req.StartDate,
            req.EndDate,
            req.OnlyActiveTrips,
            req.OrderBy,
            req.Page,
            req.PageSize);

        var items = tenantUow.Repository<Trip>()
            .ApplySpecification(spec)
            .Select(trip => trip.ToDto())
            .ToArray();

        return PagedResult<TripDto>.Succeed(items, totalItems, req.PageSize);
    }
}
