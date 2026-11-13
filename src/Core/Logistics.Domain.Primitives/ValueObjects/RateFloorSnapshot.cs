using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>The rate floor copied onto a negotiation when it opens, so later lane floor edits do not change it. Source None means no floor applied and offers cannot be checked.</summary>
public record RateFloorSnapshot(
    decimal? MinRatePerKm,
    Money? MinTotalRate,
    RateFloorSource Source)
{
    public static readonly RateFloorSnapshot None = new(null, null, RateFloorSource.None);
}
