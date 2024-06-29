using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class Invoice : Entity, ITenantEntity
{
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    public string LoadId { get; set; } = default!;
    public virtual Load Load { get; set; } = default!;

    public string CustomerId { get; set; } = default!;
    public virtual Customer Customer { get; set; } = default!;
    
    public string PaymentId { get; set; } = default!;
    public virtual Payment Payment { get; set; } = default!;
}
