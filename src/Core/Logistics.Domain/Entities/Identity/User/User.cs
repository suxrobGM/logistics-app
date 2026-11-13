using Logistics.Domain.Core;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser<Guid>, IEntity<Guid>, IMasterEntity, IAuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    /// <summary>
    /// The tenant the user signs in to by default: an employee's company, or a customer's first carrier.
    /// It also puts the tenant claim on the token, which makes every other tenant need a
    /// <see cref="UserTenantAccess"/> row - so it must stay set for customers too.
    /// </summary>
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
