using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Abstractions.Routing;

public interface IRouteDurationService
{
    /// <summary>One entry per leg, so one fewer than <paramref name="points"/>.</summary>
    Task<IReadOnlyList<TimeSpan>> GetLegDurationsAsync(IReadOnlyList<GeoPoint> points, CancellationToken ct = default);
}
