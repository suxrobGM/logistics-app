using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class Invoice : Entity, ITenantEntity
{
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    public string LoadId { get; set; } = null!;
    public virtual Load Load { get; set; } = null!;

    public string CustomerId { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
    
    public string PaymentId { get; set; } = null!;
    public virtual Payment Payment { get; set; } = null!;
}
