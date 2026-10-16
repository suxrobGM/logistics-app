using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

internal sealed class GetTripsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTripsQuery, PagedResult<TripDto>>
{
    public Task<PagedResult<TripDto>> Handle(
        GetTripsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Trip>().Query();

        if (!string.IsNullOrEmpty(req.Name))
        {
            query = query.Where(i => i.Name.Contains(req.Name));
        }

        if (req.Status.HasValue)
        {
            query = query.Where(i => i.Status == req.Status.Value);
        }

        if (!string.IsNullOrEmpty(req.TruckNumber))
        {
            query = query.Where(i => i.Truck != null && i.Truck.Number == req.TruckNumber);
        }

        if (req.TruckId.HasValue)
        {
            query = query.Where(i => i.TruckId == req.TruckId.Value);
        }

        if (req.StartDate.HasValue && req.EndDate.HasValue)
        {
            query = query.Where(i => i.CreatedAt >= req.StartDate.Value && i.CreatedAt <= req.EndDate.Value);
        }

        if (req.OnlyActiveTrips)
        {
            query = query.Where(i => i.Status != TripStatus.Completed && i.Status != TripStatus.Cancelled);
        }

        var totalItems = query.Count();

        var trips = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .ToArray();

        var trailerIds = trips.Where(t => t.TrailerId is not null).Select(t => t.TrailerId!.Value).Distinct().ToList();
        var trailerNumbers = tenantUow.Repository<Trailer>().Query()
            .Where(t => trailerIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Number })
            .ToDictionary(t => t.Id, t => t.Number);

        var items = trips
            .Select(trip => trip.ToDto(trip.TrailerId is { } id ? trailerNumbers.GetValueOrDefault(id) : null))
            .ToArray();

        return Task.FromResult(PagedResult<TripDto>.Ok(items, totalItems, req.PageSize));
    }
}
