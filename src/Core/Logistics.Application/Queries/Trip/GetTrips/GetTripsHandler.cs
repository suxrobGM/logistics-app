using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTripsHandler : RequestHandler<GetTripsQuery, PagedResult<TripDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTripsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<TripDto>> HandleValidated(
        GetTripsQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Trip>().CountAsync();
        var spec = new GetTripsSpec(req.Name, req.OrderBy, req.Page, req.PageSize);

        var items = _tenantUow.Repository<Trip>()
            .ApplySpecification(spec)
            .Select(trip => trip.ToDto())
            .ToArray();
        
        return PagedResult<TripDto>.Succeed(items, totalItems, req.PageSize);
    }
}
