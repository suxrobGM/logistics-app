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
            CreatedDate = entity.CreatedAt,
            MethodId = entity.MethodId,
            TenantId = entity.TenantId,
            Status = entity.Status,
            Description = entity.Description,
            BillingAddress = entity.BillingAddress
        };

        return dto;
    }
}
