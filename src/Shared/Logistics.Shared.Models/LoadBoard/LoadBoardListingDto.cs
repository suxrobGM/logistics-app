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

    public decimal? RatePerMile { get; set; }
    public decimal? TotalRate { get; set; }
    public string? Currency { get; set; }
    public double? Distance { get; set; }
    public int? Weight { get; set; }
    public int? Length { get; set; }

    public DateTime? PickupDateStart { get; set; }
    public DateTime? PickupDateEnd { get; set; }
    public DateTime? DeliveryDateStart { get; set; }
    public DateTime? DeliveryDateEnd { get; set; }

    public string? EquipmentType { get; set; }
    public string? Commodity { get; set; }

    public string? BrokerName { get; set; }
    public string? BrokerPhone { get; set; }
    public string? BrokerEmail { get; set; }
    public string? BrokerMcNumber { get; set; }

    public LoadBoardListingStatus Status { get; set; }
    public DateTime? BookedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public bool IsBookable => Status == LoadBoardListingStatus.Available && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// If booked, the internal Load ID
    /// </summary>
    public Guid? LoadId { get; set; }

    public string? Notes { get; set; }
}
