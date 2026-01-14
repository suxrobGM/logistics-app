using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class EldVehicleMapping : Entity, ITenantEntity
{
    public Guid TruckId { get; set; }
    public virtual Truck Truck { get; set; } = null!;

    /// <summary>
    /// Which ELD provider this mapping is for
    /// </summary>
    public required EldProviderType ProviderType { get; set; }

    /// <summary>
    /// The vehicle's ID in the ELD provider's system
    /// </summary>
    public required string ExternalVehicleId { get; set; }

    /// <summary>
    /// The vehicle's name/number as it appears in the ELD system
    /// </summary>
    public string? ExternalVehicleName { get; set; }

    /// <summary>
    /// Whether to sync data for this vehicle
    /// </summary>
    public bool IsSyncEnabled { get; set; } = true;

    /// <summary>
    /// When data was last synced for this vehicle
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}
