using Logistics.Domain.Core;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser<Guid>, IEntity<Guid>, IMasterEntity, IAuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public Guid? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// ISO 639-1 code (e.g. "en", "de"). Null means fall back to the tenant's default language.
    /// </summary>
    public string? PreferredLanguage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Set when the user submits a GDPR deletion request; cleared if cancelled within grace.
    /// </summary>
    public DateTime? DeletionRequestedAt { get; set; }

    /// <summary>
    /// Set after irreversible PII anonymization by the deletion job.
    /// Anonymized users can no longer log in.
    /// </summary>
    public DateTime? AnonymizedAt { get; set; }

    public string GetFullName()
    {
        return string.Join(" ", FirstName, LastName);
    }
}
