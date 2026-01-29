using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Driver training completion records
/// </summary>
public class TrainingRecord : AuditableEntity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public required TrainingType TrainingType { get; set; }
    public required string TrainingName { get; set; }
    public string? Provider { get; set; }

    public required DateTime CompletedDate { get; set; }

    /// <summary>
    /// Expiration date if training requires renewal
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    public decimal? Hours { get; set; }
    public string? CertificateNumber { get; set; }
    public bool IsPassed { get; set; } = true;
    public decimal? Score { get; set; }

    public string? Notes { get; set; }

    public virtual List<EmployeeDocument> Documents { get; set; } = [];
}
