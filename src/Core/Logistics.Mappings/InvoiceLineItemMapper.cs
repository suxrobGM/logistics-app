using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class InvoiceLineItemMapper
{
    extension(InvoiceLineItem entity)
    {
        public InvoiceLineItemDto ToDto()
        {
            return new InvoiceLineItemDto
            {
                Id = entity.Id,
                InvoiceId = entity.InvoiceId,
                Description = entity.Description,
                Type = entity.Type,
                Amount = entity.Amount,
                Quantity = entity.Quantity,
                Order = entity.Order,
                Notes = entity.Notes,
                Total = entity.Total,
                TaxRatePercent = entity.TaxRatePercent,
                TaxAmount = entity.TaxAmount,
                TaxCode = entity.TaxCode,
                SourceId = entity.SourceId
            };
        }
    }

    extension(IEnumerable<InvoiceLineItem> entities)
    {
        public IEnumerable<InvoiceLineItemDto> ToDto()
        {
            return entities.Select(e => e.ToDto());
        }
    }
}
