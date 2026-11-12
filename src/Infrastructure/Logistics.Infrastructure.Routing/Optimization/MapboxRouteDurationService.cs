using Logistics.Application.Abstractions.Routing;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Routing.Geocoding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Routing.Optimization;

/// <summary>
///     Leg drive times from the Mapbox Matrix API, falling back to a straight-line estimate
///     when Mapbox is not configured or the call fails.
/// </summary>
internal sealed class MapboxRouteDurationService(
    MapboxMatrixClient matrix,
    IOptions<MapboxOptions> options,
    ILogger<MapboxRouteDurationService> logger) : IRouteDurationService
{
    /// <summary>
    ///     Straight-line distance understates road distance; 1.25 and 80 km/h roughly match US truck averages.
    /// </summary>
    private const double RoadFactor = 1.25;
    private const double FallbackSpeedMetersPerSecond = 80_000 / 3600.0;

    public async Task<IReadOnlyList<TimeSpan>> GetLegDurationsAsync(
        IReadOnlyList<GeoPoint> points, CancellationToken ct = default)
    {
        if (points.Count < 2)
        {
            return [];
        }

        if (string.IsNullOrEmpty(options.Value.AccessToken))
        {
            return EstimateStraightLine(points);
        }

        try
        {
            return await GetMatrixLegsAsync(points, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Mapbox Matrix failed for ETA; using straight-line estimate");
            return EstimateStraightLine(points);
        }
    }

    /// <summary>
    ///     Splits the route into chunks of at most the coordinate cap that share their boundary point,
    ///     so every consecutive leg falls inside exactly one chunk.
    /// </summary>
    private async Task<IReadOnlyList<TimeSpan>> GetMatrixLegsAsync(IReadOnlyList<GeoPoint> points, CancellationToken ct)
    {
        var chunkSize = Math.Max(2, options.Value.MaxCoordsPerRequest);
        var legs = new List<TimeSpan>(points.Count - 1);

        for (var start = 0; start < points.Count - 1; start += chunkSize - 1)
        {
            var chunk = points.Skip(start).Take(chunkSize).Select(p => (p.Longitude, p.Latitude)).ToList();
            var result = await matrix.GetMatrixAsync(chunk, ct);

            for (var i = 0; i < chunk.Count - 1; i++)
            {
                var seconds = result.Time(i, i + 1);
                legs.Add(double.IsFinite(seconds)
                    ? TimeSpan.FromSeconds(seconds)
                    : EstimateLeg(points[start + i], points[start + i + 1]));
            }
        }

        return legs;
    }

    private static List<TimeSpan> EstimateStraightLine(IReadOnlyList<GeoPoint> points)
    {
        return [.. points.Zip(points.Skip(1), EstimateLeg)];
    }

    private static TimeSpan EstimateLeg(GeoPoint from, GeoPoint to)
    {
        return TimeSpan.FromSeconds(from.DistanceTo(to) * RoadFactor / FallbackSpeedMetersPerSecond);
    }
}
