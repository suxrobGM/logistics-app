using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// The rate floor in force when a negotiation opened, frozen so later edits to the lane floor never
/// move the bar mid-thread. <see cref="Source"/> is <see cref="RateFloorSource.None"/> when nothing
/// covered the lane, which every rate check must read as "cannot be checked" rather than "no limit".
/// </summary>
public record RateFloorSnapshot(
    decimal? MinRatePerMile,
    Money? MinTotalRate,
    RateFloorSource Source)
{
    public static readonly RateFloorSnapshot None = new(null, null, RateFloorSource.None);
}
