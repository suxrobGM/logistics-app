using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PaymentMapper
{
    public static PaymentDto ToDto(this Payment entity)
    {
        return new PaymentDto
        {
            Id = entity.Id,
            Amount = entity.Amount,
            CreatedDate = entity.CreatedAt,
            StripePaymentMethodId = entity.StripePaymentMethodId,
            TenantId = entity.TenantId,
            Status = entity.Status,
            Description = entity.Description,
            BillingAddress = entity.BillingAddress
        };
    }
}
