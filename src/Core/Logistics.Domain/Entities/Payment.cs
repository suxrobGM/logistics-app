using Logistics.Domain.Core;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Enums;

namespace Logistics.Domain.Entities;

public class Payment : Entity, ITenantEntity
{
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod? Method { get; set; }
    public PaymentFor PaymentFor { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PaymentDate { get; set; }
    public string? Comment { get; set; }
    public Address BillingAddress { get; set; } = Address.NullAddress;

    public void SetStatus(PaymentStatus status)
    {
        if (status == PaymentStatus.Paid)
        {
            PaymentDate = DateTime.UtcNow;
        }

        Status = status;
    }
}
