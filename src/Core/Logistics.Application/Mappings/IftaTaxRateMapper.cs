using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class IftaTaxRateMapper
{
    public static partial IftaTaxRateDto ToDto(this IftaTaxRate entity);
}
