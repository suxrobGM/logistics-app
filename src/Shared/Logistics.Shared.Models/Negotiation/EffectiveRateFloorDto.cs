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

    /// <summary>
    /// The per-mile and flat-total floor combined into one number for this listing's distance.
    /// Null when the listing has no distance and only a per-mile floor applies.
    /// </summary>
    public decimal? EffectiveFloorTotal { get; set; }

    public bool ListingBelowFloor { get; set; }
    public decimal? GapPerMile { get; set; }
}
