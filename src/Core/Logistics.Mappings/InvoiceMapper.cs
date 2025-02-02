using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(this Invoice entity)
    {
        return new InvoiceDto
        {
            Id = entity.Id,
            LoadRef = entity.Load.RefId,
            LoadId = entity.LoadId,
            CreatedDate = entity.CreateDate,
            Customer = entity.Customer.ToDto(),
            Payment = entity.Payment.ToDto()
        };
    }
}
