namespace Logistics.Shared.Models;

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
    public decimal? ListingRatePerKm { get; set; }
    public double? DistanceMeters { get; set; }
    public string Currency { get; set; } = "USD";
}
