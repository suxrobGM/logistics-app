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
    public decimal? MinRatePerKm { get; set; }
    public decimal? MinTotalRate { get; set; }
    public RateFloorSource Source { get; set; } = RateFloorSource.None;

    /// <summary>
    /// The per-km and flat-total floor combined into one number for this listing's distance.
    /// Null when the listing has no distance and only a per-km floor applies.
    /// </summary>
    public decimal? EffectiveFloorTotal { get; set; }

    public bool ListingBelowFloor { get; set; }
    public decimal? GapPerKm { get; set; }
}
