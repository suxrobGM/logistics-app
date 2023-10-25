using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class Invoice : AuditableEntity, ITenantEntity
{
    public string LoadId { get; set; } = default!;
    public virtual Load Load { get; set; } = default!;

    public string CustomerId { get; set; } = default!;
    public virtual Customer Customer { get; set; } = default!;
    
    public string PaymentId { get; set; } = default!;
    public virtual Payment Payment { get; set; } = default!;
}
