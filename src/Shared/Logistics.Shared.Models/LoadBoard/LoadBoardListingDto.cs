using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record LoadBoardListingDto
{
    public Guid? Id { get; set; }
    public required string ExternalListingId { get; set; }
    public LoadBoardProviderType ProviderType { get; set; }
    public string? ProviderName { get; set; }

    public required Address OriginAddress { get; set; }
    public required GeoPoint OriginLocation { get; set; }
    public required Address DestinationAddress { get; set; }
    public required GeoPoint DestinationLocation { get; set; }

    public decimal? RatePerKm { get; set; }

    /// <summary>
    /// The board's current market spot rate for this lane (DAT RateView), per km, when the
    /// tenant enabled market rates. Looked up per search, never stored.
    /// </summary>
    public decimal? MarketRatePerKm { get; set; }

    public decimal? MarketRateLowPerKm { get; set; }
    public decimal? MarketRateHighPerKm { get; set; }

    /// <summary>Rate per km minus fleet operating cost per km, driver pay and deadhead excluded.</summary>
    public decimal? EstimatedMarginPerKm { get; set; }
    public decimal? TotalRate { get; set; }
    public string? Currency { get; set; }
    public double? Distance { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? LengthMeters { get; set; }

    public DateTime? PickupDateStart { get; set; }
    public DateTime? PickupDateEnd { get; set; }
    public DateTime? DeliveryDateStart { get; set; }
    public DateTime? DeliveryDateEnd { get; set; }

    public LoadBoardEquipmentType? EquipmentType { get; set; }
    public string? Commodity { get; set; }

    public string? BrokerName { get; set; }
    public string? BrokerPhone { get; set; }
    public string? BrokerEmail { get; set; }
    public string? BrokerMcNumber { get; set; }

    /// <summary>
    /// Broker credit score normalized to 0-100, when the provider or a credit check supplied one.
    /// </summary>
    public int? BrokerCreditScore { get; set; }

    /// <summary>
    /// Average days the broker takes to pay carriers.
    /// </summary>
    public int? BrokerDaysToPay { get; set; }

    public DateTime? BrokerCreditCheckedAt { get; set; }

    public LoadBoardListingStatus Status { get; set; }
    public DateTime? BookedAt { get; set; }
    /// <summary>
    /// When the posting expires. Providers leave it null when the board does not say; the search
    /// handler then stamps a short shelf life so stale listings still get cleaned up.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    public bool IsBookable => Status == LoadBoardListingStatus.Available &&
                              (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

    /// <summary>
    /// If booked, the internal Load ID
    /// </summary>
    public Guid? LoadId { get; set; }

    public string? Notes { get; set; }
}
