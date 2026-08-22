namespace Logistics.Shared.Models;

/// <summary>
/// Everything needed to decide whether a listing is worth negotiating: the floor in force, how the
/// listing compares to it, and whether a thread is already running.
/// </summary>
public record RateFloorContextDto
{
    public Guid ListingId { get; set; }
    public EffectiveRateFloorDto Floor { get; set; } = new();

    /// <summary>Whether the listing carries a broker address. Without one no offer can be sent.</summary>
    public bool BrokerEmailAvailable { get; set; }

    public Guid? ActiveNegotiationId { get; set; }
    public int RoundCount { get; set; }
    public int MaxRounds { get; set; }

    public decimal? ListingTotalRate { get; set; }
    public decimal? ListingRatePerMile { get; set; }
    public double? DistanceMiles { get; set; }
    public string Currency { get; set; } = "USD";
}
