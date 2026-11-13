using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Financial.Invoices;

[Mapper]
public static partial class InvoiceTaxLineMapper
{
    public static partial InvoiceTaxLineDto ToDto(this InvoiceTaxLine entity);

    public static partial TaxJurisdictionDto ToDto(this TaxJurisdiction entity);

    extension(IEnumerable<InvoiceTaxLine> entities)
    {
        public IEnumerable<InvoiceTaxLineDto> ToDto() =>
            entities.Select(ToDto);
    }
}
