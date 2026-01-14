using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class HosViolation : Entity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    /// <summary>
    /// When the violation occurred
    /// </summary>
    public required DateTime ViolationDate { get; set; }

    /// <summary>
    /// The type of HOS violation
    /// </summary>
    public required HosViolationType ViolationType { get; set; }

    /// <summary>
    /// Human-readable description of the violation
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Severity level from 1 (minor) to 5 (critical)
    /// </summary>
    public int SeverityLevel { get; set; } = 1;

    /// <summary>
    /// Whether this violation has been resolved/addressed
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// When the violation was resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// The violation ID in the ELD provider's system
    /// </summary>
    public string? ExternalViolationId { get; set; }

    /// <summary>
    /// Which ELD provider reported this violation
    /// </summary>
    public EldProviderType ProviderType { get; set; }
}
