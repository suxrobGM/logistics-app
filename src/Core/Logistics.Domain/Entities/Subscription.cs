using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class Subscription : AuditableEntity
{
    public required string TenantId { get; set; }
    public virtual required Tenant Tenant { get; set; }
    public bool IsTrial { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
