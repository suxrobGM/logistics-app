using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PaymentMapper
{
    extension(Payment entity)
    {
        public PaymentDto ToDto()
        {
            return new PaymentDto
            {
                Id = entity.Id,
                Amount = entity.Amount,
                CreatedDate = entity.CreatedAt,
                StripePaymentMethodId = entity.StripePaymentMethodId,
                Status = entity.Status,
                Description = entity.Description,
                BillingAddress = entity.BillingAddress
            };
        }
    }
}
