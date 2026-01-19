using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a truck that has been posted to external load boards (DAT, Truckstop, 123Loadboard)
/// to advertise available capacity to shippers and brokers.
/// </summary>
/// <remarks>
/// When a trucking company has an available truck, they can "post" it to load boards
/// to let brokers know the truck is looking for freight. This is the carrier-side equivalent
/// of load listings - instead of searching for loads, carriers advertise their available trucks.
/// <para>
/// Key concepts:
/// <list type="bullet">
/// <item><description>A posted truck advertises where the truck is available and when</description></item>
/// <item><description>Carriers can specify destination preferences to find loads going a certain direction</description></item>
/// <item><description>Posts typically expire after a set period and need periodic refresh</description></item>
/// <item><description>Brokers see posted trucks and can contact the carrier with load opportunities</description></item>
/// </list>
/// </para>
/// </remarks>
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
    /// The load board provider where this truck is posted (DAT, Truckstop, 123Loadboard, Demo)
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
    /// Radius in miles willing to travel for pickup
    /// </summary>
    public int? DestinationRadius { get; set; }

    /// <summary>
    /// When the truck becomes available
    /// </summary>
    public DateTime AvailableFrom { get; set; }

    /// <summary>
    /// When the truck availability window ends
    /// </summary>
    public DateTime? AvailableTo { get; set; }

    /// <summary>
    /// Equipment type (e.g., "Flatbed", "Dry Van", "Reefer")
    /// </summary>
    public string? EquipmentType { get; set; }

    /// <summary>
    /// Maximum weight capacity in pounds
    /// </summary>
    public int? MaxWeight { get; set; }

    /// <summary>
    /// Maximum length capacity in feet
    /// </summary>
    public int? MaxLength { get; set; }

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
