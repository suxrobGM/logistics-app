using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Driver certification/license tracking (CDL, medical certificates, endorsements)
/// </summary>
public class DriverCertification : AuditableEntity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public required CertificationType CertificationType { get; set; }
    public required string CertificationNumber { get; set; }
    public required string IssuingAuthority { get; set; }
    public string? IssuingState { get; set; }

    public required DateTime IssuedDate { get; set; }
    public required DateTime ExpirationDate { get; set; }

    public CertificationStatus Status { get; set; } = CertificationStatus.Active;

    /// <summary>
    /// For CDL - the license class
    /// </summary>
    public CdlClass? CdlClass { get; set; }

    /// <summary>
    /// Endorsements as JSON array (e.g., ["H", "N", "P", "S", "T"])
    /// </summary>
    public string? Endorsements { get; set; }

    /// <summary>
    /// Restrictions as JSON array
    /// </summary>
    public string? Restrictions { get; set; }

    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedById { get; set; }
    public virtual Employee? VerifiedBy { get; set; }

    public string? Notes { get; set; }

    public virtual List<EmployeeDocument> Documents { get; set; } = [];

    public bool IsExpired => ExpirationDate < DateTime.UtcNow;
    public bool IsExpiringSoon => !IsExpired && ExpirationDate <= DateTime.UtcNow.AddDays(30);
    public int DaysUntilExpiration => (int)(ExpirationDate - DateTime.UtcNow).TotalDays;
}
