using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(this Invoice entity)
    {
        return new InvoiceDto
        {
            Customer = entity.Customer.ToDto(),
            LoadId = entity.LoadId,
            Payment = entity.Payment.ToDto()
        };
    }
}
