using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record EffectiveRateFloorDto
{
    public bool HasFloor { get; set; }
    public decimal? MinRatePerKm { get; set; }
    public decimal? MinTotalRate { get; set; }
    public RateFloorSource Source { get; set; } = RateFloorSource.None;

    /// <summary>Floor as a total amount for the listing; null when the listing has no distance and only a per-km floor applies.</summary>
    public decimal? EffectiveFloorTotal { get; set; }

    public bool ListingBelowFloor { get; set; }
    public decimal? GapPerKm { get; set; }
}
