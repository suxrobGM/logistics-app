using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class InvoiceTaxLineMapper
{
    public static partial InvoiceTaxLineDto ToDto(this InvoiceTaxLine entity);

    public static partial TaxJurisdictionDto ToDto(this TaxJurisdiction entity);

    public static IEnumerable<InvoiceTaxLineDto> ToDto(this IEnumerable<InvoiceTaxLine> entities) =>
        entities.Select(ToDto);
}
