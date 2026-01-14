using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class EldDriverMapping : Entity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    /// <summary>
    /// Which ELD provider this mapping is for
    /// </summary>
    public required EldProviderType ProviderType { get; set; }

    /// <summary>
    /// The driver's ID in the ELD provider's system
    /// </summary>
    public required string ExternalDriverId { get; set; }

    /// <summary>
    /// The driver's name as it appears in the ELD system
    /// </summary>
    public string? ExternalDriverName { get; set; }

    /// <summary>
    /// Whether to sync HOS data for this driver
    /// </summary>
    public bool IsSyncEnabled { get; set; } = true;

    /// <summary>
    /// When data was last synced for this driver
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}
