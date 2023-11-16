using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class PaymentMapper
{
    public static PaymentDto ToDto(this Payment entity)
    {
        var dto = new PaymentDto
        {
            Id = entity.Id,
            Amount = entity.Amount,
            CreatedDate = entity.CreatedDate,
            Comment = entity.Comment,
            Method = entity.Method,
            PaymentDate = entity.PaymentDate,
            PaymentFor = entity.PaymentFor,
            Status = entity.Status,
        };
        
        if (entity.BillingAddress.IsNotNull())
        {
            dto.BillingAddress = entity.BillingAddress.ToDto();
        }
        return dto;
    }
}
