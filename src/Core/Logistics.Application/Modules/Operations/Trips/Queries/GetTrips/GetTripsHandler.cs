using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Common.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

internal sealed class GetTripsHandler(ITenantUnitOfWork tenantUow, IDisplayNameLookup names)
    : IRequestHandler<GetTripsQuery, PagedResult<TripDto>>
{
    public Task<PagedResult<TripDto>> Handle(
        GetTripsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Trip>().Query();

        if (!string.IsNullOrEmpty(req.Name))
        {
            var name = req.Name.ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(name));
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

        return query.ToPagedResultAsync(req, MapPageAsync, ct);
    }

    private async Task<IEnumerable<TripDto>> MapPageAsync(IReadOnlyList<Trip> trips, CancellationToken ct)
    {
        var tripIds = trips.Select(t => t.Id).ToArray();
        var truckNumbers = await names.GetTruckNumbersAsync(trips.Select(t => t.TruckId), ct);
        var trailerNumbers = await GetTrailerNumbersAsync(trips, ct);

        var stops = await tenantUow.Repository<TripStop>().Query()
            .Where(s => tripIds.Contains(s.TripId))
            .OrderBy(s => s.Order)
            .ToListAsync(ct);
        var stopsByTrip = stops.ToLookup(s => s.TripId);
        var loads = await GetTripLoadsAsync(stops, ct);

        return trips.Select(trip =>
        {
            var tripStops = stopsByTrip[trip.Id].ToArray();
            var tripLoads = tripStops.Select(s => s.LoadId)
                .Distinct()
                .Where(loads.ContainsKey)
                .Select(id => loads[id])
                .ToArray();

            return trip.ToDto(
                truckNumbers.Find(trip.TruckId),
                trailerNumbers.Find(trip.TrailerId),
                tripStops,
                tripLoads);
        });
    }

    private async Task<Dictionary<Guid, string>> GetTrailerNumbersAsync(IReadOnlyList<Trip> trips, CancellationToken ct)
    {
        var trailerIds = trips.Select(t => t.TrailerId).OfType<Guid>().Distinct().ToArray();
        return await tenantUow.Repository<Trailer>().Query()
            .Where(t => trailerIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Number, ct);
    }

    private async Task<Dictionary<Guid, TripLoadDto>> GetTripLoadsAsync(List<TripStop> stops, CancellationToken ct)
    {
        var loadIds = stops.Select(s => s.LoadId).Distinct().ToArray();
        var loads = await tenantUow.Repository<Load>().Query()
            .Where(l => loadIds.Contains(l.Id))
            .ToListAsync(ct);

        var customerIds = loads.Select(l => l.CustomerId).Distinct().ToArray();
        var customers = await tenantUow.Repository<Customer>().Query()
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.ToDto(), ct);

        return loads.ToDictionary(l => l.Id, l => l.ToTripLoadDto(customers[l.CustomerId]));
    }
}
