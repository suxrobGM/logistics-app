using Logistics.Domain.Core;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser<Guid>, IEntity<Guid>, IMasterEntity, IAuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    /// <summary>Tenant the user signs in to by default; set for customer users too. Access to any other tenant requires a <see cref="UserTenantAccess"/> row.</summary>
    public Guid? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

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
