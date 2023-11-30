using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class SubscriptionPaymentMapper
{
    public static SubscriptionPaymentDto ToDto(this SubscriptionPayment entity)
    {
        return new SubscriptionPaymentDto
        {
            Id = entity.Id,
            Amount = entity.Amount,
            Status = entity.Status,
            Method = entity.Method,
            CreatedDate = entity.CreatedDate,
            PaymentDate = entity.PaymentDate,
            BillingAddress = entity.BillingAddress.ToDto(),
            Subscription = entity.Subscription.ToDto()
        };
    }
}
