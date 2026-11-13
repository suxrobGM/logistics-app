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

    /// <summary>Market rate per km for the listing's lane, looked up on each search when market rates are enabled and never stored.</summary>
    public decimal? MarketRatePerKm { get; set; }

    public decimal? MarketRateLowPerKm { get; set; }
    public decimal? MarketRateHighPerKm { get; set; }

    /// <summary>Listing rate per km minus the fleet operating cost per km, excluding driver pay and deadhead.</summary>
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
    /// <summary>When the posting expires. Boards that give no expiry leave it null, and the search handler then sets a default shelf life.</summary>
    public DateTime? ExpiresAt { get; set; }

    public bool IsBookable => Status == LoadBoardListingStatus.Available &&
                              (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

    /// <summary>
    /// If booked, the internal Load ID
    /// </summary>
    public Guid? LoadId { get; set; }

    public string? Notes { get; set; }
}
