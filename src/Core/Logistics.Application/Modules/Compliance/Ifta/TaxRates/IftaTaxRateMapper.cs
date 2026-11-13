using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates;

[Mapper]
public static partial class IftaTaxRateMapper
{
    public static partial IftaTaxRateDto ToDto(this IftaTaxRate entity);
}
