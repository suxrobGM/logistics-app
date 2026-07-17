using Logistics.Application.Abstractions.Geocoding;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

internal sealed class TruckLocationRecorder(
    ITenantUnitOfWork tenantUow,
    IJurisdictionResolver jurisdictionResolver,
    ILogger<TruckLocationRecorder> logger) : ITruckLocationRecorder
{
    private const double MetersPerMile = 1609.344;

    /// <summary>Segments implying more than this speed are GPS teleports and are discarded.</summary>
    private const double MaxPlausibleSpeedMph = 90;

    /// <summary>Segments shorter than this are stationary jitter and accrue nothing.</summary>
    private const decimal MinSegmentMiles = 0.05m;

    public async Task RecordAsync(
        Truck truck,
        GeoPoint location,
        DateTime timestampUtc,
        EldProviderType? source = null,
        int? odometerReading = null,
        CancellationToken ct = default)
    {
        var previous = await tenantUow.Repository<TruckLocationHistory>().Query()
            .Where(h => h.TruckId == truck.Id)
            .OrderByDescending(h => h.Timestamp)
            .FirstOrDefaultAsync(ct);

        // Out-of-order or duplicate ping: keep the newest state, don't accrue backwards
        if (previous is not null && timestampUtc <= previous.Timestamp)
        {
            return;
        }

        await tenantUow.Repository<TruckLocationHistory>().AddAsync(new TruckLocationHistory
        {
            TruckId = truck.Id,
            Location = location,
            Timestamp = timestampUtc,
            Source = source,
            OdometerReading = odometerReading
        }, ct);

        truck.CurrentLocation = location;

        if (previous is not null)
        {
            await AccrueSegmentAsync(truck, previous, location, timestampUtc, odometerReading, ct);
        }
    }

    private async Task AccrueSegmentAsync(
        Truck truck,
        TruckLocationHistory previous,
        GeoPoint location,
        DateTime timestampUtc,
        int? odometerReading,
        CancellationToken ct)
    {
        var haversineMiles = (decimal)(previous.Location.DistanceTo(location) / MetersPerMile);
        var miles = ResolveSegmentMiles(previous.OdometerReading, odometerReading, haversineMiles);

        if (miles < MinSegmentMiles)
        {
            return;
        }

        // Teleport guard: an implausible implied speed means a GPS glitch, not real miles.
        // Long gaps still accrue — dropping miles understates tax liability, which is worse
        // in an IFTA audit than GPS drift.
        var hours = (timestampUtc - previous.Timestamp).TotalHours;
        if (hours > 0 && (double)miles / hours > MaxPlausibleSpeedMph)
        {
            logger.LogWarning(
                "Discarding GPS segment for truck {TruckNumber}: {Miles:F1} mi in {Hours:F2} h is implausible",
                truck.Number, miles, hours);
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
        var rollupRepo = tenantUow.Repository<TruckJurisdictionMileage>();
        var rollup = await rollupRepo.GetAsync(m =>
            m.TruckId == truck.Id &&
            m.Date == date &&
            m.Jurisdiction.CountryCode == jurisdiction.CountryCode &&
            m.Jurisdiction.Region == jurisdiction.Region, ct);

        if (rollup is null)
        {
            await rollupRepo.AddAsync(new TruckJurisdictionMileage
            {
                TruckId = truck.Id,
                Jurisdiction = jurisdiction,
                Date = date,
                Miles = Math.Round(miles, 2)
            }, ct);
        }
        else
        {
            rollup.Miles += Math.Round(miles, 2);
        }
    }

    /// <summary>
    /// Odometer delta wins when both pings carry a plausible reading (within 2× haversine + 5 mi,
    /// covering road-vs-chord distance); otherwise fall back to the haversine chord.
    /// </summary>
    private static decimal ResolveSegmentMiles(int? previousOdometer, int? currentOdometer, decimal haversineMiles)
    {
        if (previousOdometer.HasValue && currentOdometer.HasValue)
        {
            var delta = currentOdometer.Value - previousOdometer.Value;
            if (delta >= 0 && delta <= (double)(haversineMiles * 2 + 5))
            {
                return delta;
            }
        }

        return haversineMiles;
    }
}
