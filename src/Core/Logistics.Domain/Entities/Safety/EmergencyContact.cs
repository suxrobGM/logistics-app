using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Emergency contact for notifications
/// </summary>
public class EmergencyContact : Entity, ITenantEntity
{
    public required string Name { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public required EmergencyContactType ContactType { get; set; }

    /// <summary>
    /// Order in the contact chain (lower = higher priority)
    /// </summary>
    public required int Priority { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// If set, this contact is specific to a driver. Otherwise, it's tenant-wide.
    /// </summary>
    public Guid? EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }
}
