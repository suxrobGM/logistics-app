using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class SubscriptionPaymentMapper
{
    public static SubscriptionPaymentDto ToDto(this SubscriptionPayment entity)
    {
        return new SubscriptionPaymentDto
        {
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Payment = entity.Payment.ToDto()
        };
    }
}
