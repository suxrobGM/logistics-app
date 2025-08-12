using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTripsHandler : IAppRequestHandler<GetTripsQuery, PagedResult<TripDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetTripsHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<PagedResult<TripDto>> Handle(
        GetTripsQuery req, CancellationToken ct)
    {
        var totalItems = await _tenantUow.Repository<Trip>().CountAsync();
        var spec = new GetTripsSpec(req.Name, req.Status, req.TruckNumber, req.OrderBy, req.Page, req.PageSize);

        var items = _tenantUow.Repository<Trip>()
            .ApplySpecification(spec)
            .Select(trip => trip.ToDto())
            .ToArray();

        return PagedResult<TripDto>.Succeed(items, totalItems, req.PageSize);
    }
}
