using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PaymentMapper
{
    public static PaymentDto ToDto(this Payment entity)
    {
        var dto = new PaymentDto
        {
            Id = entity.Id,
            Amount = entity.Amount,
            CreatedDate = entity.CreatedDate,
            Notes = entity.Notes,
            Method = entity.Method,
            PaymentDate = entity.PaymentDate,
            PaymentFor = entity.PaymentFor,
            Status = entity.Status,
            SubscriptionId = entity.SubscriptionId,
        };
        
        if (entity.BillingAddress.IsNotNull())
        {
            dto.BillingAddress = entity.BillingAddress.ToDto();
        }
        return dto;
    }
}
