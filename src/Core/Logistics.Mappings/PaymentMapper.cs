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
            Amount = entity.Amount.ToDto(),
            CreatedAt = entity.CreatedAt,
            Method = entity.Method,
            Status = entity.Status,
        };
        
        if (entity.BillingAddress.IsNotNull())
        {
            dto.BillingAddress = entity.BillingAddress.ToDto();
        }
        return dto;
    }
}
