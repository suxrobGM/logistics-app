using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(this Invoice entity)
    {
        return new InvoiceDto
        {
            LoadRef = entity.Load.RefId,
            LoadId = entity.LoadId,
            CreatedDate = entity.Created,
            Customer = entity.Customer.ToDto(),
            Payment = entity.Payment.ToDto()
        };
    }
}
