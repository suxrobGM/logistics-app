using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// A physical fuel card mapped to a truck so synced transactions post to the right unit.
/// Cards can be unassigned (TruckId null) while awaiting a dispatcher's mapping.
/// </summary>
public class FuelCard : AuditableEntity, ITenantEntity
{
    public required FuelCardProviderType ProviderType { get; set; }

    /// <summary>
    /// Provider-side card identifier (card ID or masked card number). Unique per provider.
    /// </summary>
    public required string ExternalCardId { get; set; }

    /// <summary>
    /// Unit/vehicle number the provider associates with this card, when available.
    /// </summary>
    public string? UnitNumber { get; set; }

    public Guid? TruckId { get; set; }
    public virtual Truck? Truck { get; set; }

    public bool IsActive { get; set; } = true;
}
