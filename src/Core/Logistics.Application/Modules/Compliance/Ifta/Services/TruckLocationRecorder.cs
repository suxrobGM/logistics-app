using Logistics.Application.Abstractions.Routing;
using Logistics.Application.Modules.Operations.Trips.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

internal sealed class TruckLocationRecorder(
    ITenantUnitOfWork tenantUow,
    IJurisdictionResolver jurisdictionResolver,
    IEtaService etaService,
    ITripAlertReporter tripAlertReporter,
    ILogger<TruckLocationRecorder> logger) : ITruckLocationRecorder
{
    /// <summary>Segments implying more than this speed (about 90 mph) are GPS teleports and are discarded.</summary>
    private const double MaxPlausibleMetersPerHour = 145_000;

    /// <summary>Segments shorter than this are stationary jitter and accrue nothing.</summary>
    private const decimal MinSegmentMeters = 80m;

    /// <summary>Slack over twice the chord before an odometer delta is distrusted (about 5 mi).</summary>
    private const double OdometerToleranceMeters = 8_000;

    /// <summary>Moves shorter than this keep the current address; parked trucks jitter by a few metres.</summary>
    private const double AddressStaleMeters = 500;

    public async Task RecordAsync(
        Truck truck,
        GeoPoint location,
        DateTime timestampUtc,
        EldProviderType? source = null,
        long? odometerMeters = null,
        Address? address = null,
        CancellationToken ct = default)
    {
        var previous = await tenantUow.Repository<TruckLocationHistory>().Query()
            .Where(h => h.TruckId == truck.Id)
            .OrderByDescending(h => h.RecordedAt)
            .FirstOrDefaultAsync(ct);

        // Out-of-order or duplicate ping: keep the newest state, don't accrue backwards
        if (previous is not null && timestampUtc <= previous.RecordedAt)
        {
            return;
        }

        await tenantUow.Repository<TruckLocationHistory>().AddAsync(new TruckLocationHistory
        {
            TruckId = truck.Id,
            Location = location,
            RecordedAt = timestampUtc,
            Source = source,
            OdometerMeters = odometerMeters
        }, ct);

        var addressStale = truck.CurrentLocation is not { } current
                           || current.DistanceTo(location) > AddressStaleMeters;
        if (address is not null || addressStale)
        {
            truck.CurrentAddress = address;
        }

        truck.CurrentLocation = location;
        if (await etaService.RefreshAsync(truck, timestampUtc, ct) is { } trip)
        {
            await tripAlertReporter.CheckHosShortfallAsync(trip, ct);
        }

        if (previous is not null)
        {
            await AccrueSegmentAsync(truck, previous, location, timestampUtc, odometerMeters, ct);
        }
    }

    private async Task AccrueSegmentAsync(
        Truck truck,
        TruckLocationHistory previous,
        GeoPoint location,
        DateTime timestampUtc,
        long? odometerMeters,
        CancellationToken ct)
    {
        var haversineMeters = previous.Location.DistanceTo(location);
        var meters = ResolveSegmentMeters(previous.OdometerMeters, odometerMeters, haversineMeters);

        if (meters < MinSegmentMeters)
        {
            return;
        }

        // Teleport guard: an implausible implied speed means a GPS glitch, not real distance.
        // Long gaps still accrue - dropping distance understates tax liability, which is worse
        // in an IFTA audit than GPS drift.
        var hours = (timestampUtc - previous.RecordedAt).TotalHours;
        if (hours > 0 && (double)meters / hours > MaxPlausibleMetersPerHour)
        {
            logger.LogWarning(
                "Discarding GPS segment for truck {TruckNumber}: {Kilometers:F1} km in {Hours:F2} h is implausible",
                truck.Number, meters / 1000m, hours);
            return;
        }

        // The whole segment is attributed to the destination ping's jurisdiction; at 5-minute
        // ping granularity the error is immaterial. Unresolvable points (offshore/glitch) fall
        // back to the segment origin's jurisdiction.
        var jurisdiction = jurisdictionResolver.Resolve(location) ?? jurisdictionResolver.Resolve(previous.Location);
        if (jurisdiction is null)
        {
            return;
        }

        var date = DateOnly.FromDateTime(timestampUtc);
        var segmentMeters = Math.Round(meters, 2);

        // One atomic statement against the (truck, date, jurisdiction) unique index, NULLS NOT DISTINCT on
        // region: concurrent pings for the same day add up instead of racing a read-then-insert.
        await tenantUow.ExecuteSqlAsync($"""
            INSERT INTO truck_jurisdiction_mileage
                (id, truck_id, date, meters, jurisdiction_country_code, jurisdiction_region)
            VALUES ({Guid.CreateVersion7()}, {truck.Id}, {date}, {segmentMeters}, {jurisdiction.CountryCode}, {jurisdiction.Region})
            ON CONFLICT (truck_id, date, jurisdiction_country_code, jurisdiction_region)
            DO UPDATE SET meters = truck_jurisdiction_mileage.meters + excluded.meters
            """, ct);
    }

    /// <summary>
    /// Odometer delta wins when both pings carry a plausible reading (within 2× haversine plus
    /// <see cref="OdometerToleranceMeters"/>, covering road-vs-chord distance); otherwise the chord.
    /// </summary>
    private static decimal ResolveSegmentMeters(long? previousOdometer, long? currentOdometer, double haversineMeters)
    {
        if (previousOdometer.HasValue && currentOdometer.HasValue)
        {
            var delta = currentOdometer.Value - previousOdometer.Value;
            if (delta >= 0 && delta <= haversineMeters * 2 + OdometerToleranceMeters)
            {
                return delta;
            }
        }

        return haversineMeters.ToDecimal();
    }
}
