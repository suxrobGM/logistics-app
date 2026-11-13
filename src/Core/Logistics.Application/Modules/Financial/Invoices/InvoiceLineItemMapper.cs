using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Financial.Invoices;

[Mapper]
public static partial class InvoiceLineItemMapper
{
    [MapperIgnoreSource(nameof(InvoiceLineItem.Invoice))]
    public static partial InvoiceLineItemDto ToDto(this InvoiceLineItem entity);

    extension(IEnumerable<InvoiceLineItem> entities)
    {
        public IEnumerable<InvoiceLineItemDto> ToDto()
        {
            return entities.Select(e => e.ToDto());
        }
    }
}
