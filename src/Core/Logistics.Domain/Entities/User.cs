using Logistics.Domain.Core;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser<Guid>, IEntity<Guid>, IMasterEntity, IAuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    
    public Guid? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public string GetFullName()
    {
        return string.Join(" ", FirstName, LastName);
    }
}
