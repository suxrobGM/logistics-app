using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>A truck advertised on an external load board so brokers can offer it freight.</summary>
public class PostedTruck : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// Foreign key to the internal Truck entity being advertised
    /// </summary>
    public Guid TruckId { get; set; }

    /// <summary>
    /// Navigation property to the internal Truck entity
    /// </summary>
    public virtual required Truck Truck { get; set; }

    /// <summary>
    /// The load board provider where this truck is posted (DAT or Demo)
    /// </summary>
    public required LoadBoardProviderType ProviderType { get; set; }

    /// <summary>
    /// The post ID assigned by the external load board provider
    /// </summary>
    public string? ExternalPostId { get; set; }

    /// <summary>
    /// Location where the truck is available
    /// </summary>
    public required Address AvailableAtAddress { get; set; }
    public required GeoPoint AvailableAtLocation { get; set; }

    /// <summary>
    /// Preferred destination area (optional)
    /// </summary>
    public Address? DestinationPreference { get; set; }

    /// <summary>
    /// Radius in meters willing to travel for pickup
    /// </summary>
    public int? DestinationRadiusMeters { get; set; }

    /// <summary>
    /// When the truck becomes available
    /// </summary>
    public DateTime AvailableFrom { get; set; }

    /// <summary>
    /// When the truck availability window ends
    /// </summary>
    public DateTime? AvailableTo { get; set; }

    /// <summary>
    /// Equipment the truck is posted with
    /// </summary>
    public LoadBoardEquipmentType? EquipmentType { get; set; }

    /// <summary>
    /// Maximum weight capacity in kilograms
    /// </summary>
    public decimal? MaxWeightKg { get; set; }

    /// <summary>
    /// Maximum length capacity in meters
    /// </summary>
    public decimal? MaxLengthMeters { get; set; }

    /// <summary>
    /// Current status of the posted truck (Available, Booked, Hidden, Expired)
    /// </summary>
    public PostedTruckStatus Status { get; set; } = PostedTruckStatus.Available;

    /// <summary>
    /// When this post expires on the load board
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// When the post was last refreshed on the load board
    /// </summary>
    public DateTime? LastRefreshedAt { get; set; }
}
