using Logistics.Application.Abstractions.Routing;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trips.Services;

internal sealed class EtaService(ITenantUnitOfWork tenantUow, IRouteDurationService routeDurations) : IEtaService
{
    private const int DefaultDwellMinutes = 60;

    public async Task<Trip?> RefreshAsync(Truck truck, DateTime now, CancellationToken ct = default)
    {
        if (truck.CurrentLocation is not { } location)
        {
            return null;
        }

        var trip = await tenantUow.Repository<Trip>().Query()
            .Where(t => t.TruckId == truck.Id)
            .Where(t => t.Status == TripStatus.Dispatched || t.Status == TripStatus.InTransit)
            .OrderByDescending(t => t.DispatchedAt)
            .FirstOrDefaultAsync(ct);

        if (trip is null || !trip.IsEtaDue(now))
        {
            return null;
        }

        var stops = trip.GetRemainingStops();
        if (stops.Count == 0)
        {
            return null;
        }

        var legs = await routeDurations.GetLegDurationsAsync([location, .. stops.Select(s => s.Location)], ct);
        var dwell = GetDwellTime();
        var etaByStopId = new Dictionary<Guid, DateTime>();
        var arrival = now;

        for (var i = 0; i < stops.Count && i < legs.Count; i++)
        {
            arrival += legs[i];
            etaByStopId[stops[i].Id] = arrival;
            arrival += dwell;
        }

        trip.ApplyArrivalEstimates(etaByStopId, now);
        return trip;
    }

    public TimeSpan? GetRemainingDriveTime(Trip trip, DateTime now)
    {
        var stops = trip.GetRemainingStops();
        if (stops.Count == 0 || stops[^1].EstimatedArrivalAt is not { } lastEta)
        {
            return null;
        }

        var driveTime = lastEta - now - GetDwellTime() * (stops.Count - 1);
        return driveTime > TimeSpan.Zero ? driveTime : TimeSpan.Zero;
    }

    private TimeSpan GetDwellTime()
    {
        var minutes = tenantUow.GetCurrentTenant().Settings.StopDwellMinutes ?? DefaultDwellMinutes;
        return TimeSpan.FromMinutes(minutes);
    }
}
