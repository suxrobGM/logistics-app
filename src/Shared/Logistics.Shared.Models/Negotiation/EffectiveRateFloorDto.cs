using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>
/// The rate floor resolved for a specific load board listing, plus how the listing's own rate
/// compares to it.
/// </summary>
public record EffectiveRateFloorDto
{
    public bool HasFloor { get; set; }
    public decimal? MinRatePerMile { get; set; }
    public Money? MinTotalRate { get; set; }
    public RateFloorSource Source { get; set; } = RateFloorSource.None;
    public Guid? MatchedLaneId { get; set; }
    public bool ListingBelowFloor { get; set; }
    public decimal? GapPerMile { get; set; }
}
