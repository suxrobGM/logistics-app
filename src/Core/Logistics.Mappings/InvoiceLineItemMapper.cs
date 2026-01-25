using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class InvoiceLineItemMapper
{
    public static InvoiceLineItemDto ToDto(this InvoiceLineItem entity)
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
            Total = entity.Total
        };
    }

    public static IEnumerable<InvoiceLineItemDto> ToDto(this IEnumerable<InvoiceLineItem> entities)
    {
        return entities.Select(e => e.ToDto());
    }
}
