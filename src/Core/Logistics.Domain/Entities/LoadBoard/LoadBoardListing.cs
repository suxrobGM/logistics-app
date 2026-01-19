using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents an external load found on load boards.
/// </summary>
public class LoadBoardListing : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// Unique identifier from the external load board provider
    /// </summary>
    public required string ExternalListingId { get; set; }

    public required LoadBoardProviderType ProviderType { get; set; }

    public required Address OriginAddress { get; set; }
    public required GeoPoint OriginLocation { get; set; }

    public required Address DestinationAddress { get; set; }
    public required GeoPoint DestinationLocation { get; set; }

    /// <summary>
    /// Rate per mile offered for this load
    /// </summary>
    public decimal? RatePerMile { get; set; }

    /// <summary>
    /// Total rate for the load
    /// </summary>
    public Money? TotalRate { get; set; }

    /// <summary>
    /// Total distance in miles
    /// </summary>
    public double? Distance { get; set; }

    /// <summary>
    /// Load weight in pounds
    /// </summary>
    public int? Weight { get; set; }

    /// <summary>
    /// Load length in feet
    /// </summary>
    public int? Length { get; set; }

    public DateTime? PickupDateStart { get; set; }
    public DateTime? PickupDateEnd { get; set; }
    public DateTime? DeliveryDateStart { get; set; }
    public DateTime? DeliveryDateEnd { get; set; }

    /// <summary>
    /// Equipment type required (e.g., "Flatbed", "Dry Van", "Reefer")
    /// </summary>
    public string? EquipmentType { get; set; }

    /// <summary>
    /// Description of the commodity being transported
    /// </summary>
    public string? Commodity { get; set; }

    public string? BrokerName { get; set; }
    public string? BrokerPhone { get; set; }
    public string? BrokerEmail { get; set; }
    public string? BrokerMcNumber { get; set; }

    public LoadBoardListingStatus Status { get; set; } = LoadBoardListingStatus.Available;

    public DateTime? BookedAt { get; set; }

    /// <summary>
    /// Link to internal Load when booked
    /// </summary>
    public Guid? LoadId { get; set; }
    public virtual Load? Load { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Store original provider response for debugging
    /// </summary>
    public string? RawJson { get; set; }

    /// <summary>
    /// When this listing expires on the load board
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
