using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Abstractions.Routing;

/// <summary>
///     Drive times along an ordered route.
/// </summary>
public interface IRouteDurationService
{
    /// <summary>
    ///     Returns the drive time of each leg between consecutive <paramref name="points"/>,
    ///     so the result has one entry fewer than the input.
    /// </summary>
    Task<IReadOnlyList<TimeSpan>> GetLegDurationsAsync(IReadOnlyList<GeoPoint> points, CancellationToken ct = default);
}
