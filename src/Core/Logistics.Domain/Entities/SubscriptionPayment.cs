using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class SubscriptionPayment : Entity, ITenantEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string PaymentId { get; set; } = default!;
    public virtual Payment Payment { get; set; } = default!;
}
