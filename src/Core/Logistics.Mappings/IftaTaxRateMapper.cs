using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class IftaTaxRateMapper
{
    [MapperIgnoreSource(nameof(IftaTaxRate.DomainEvents))]
    [MapperIgnoreSource(nameof(IftaTaxRate.UpdatedAt))]
    [MapperIgnoreSource(nameof(IftaTaxRate.UpdatedBy))]
    [MapperIgnoreSource(nameof(IftaTaxRate.CreatedBy))]
    public static partial IftaTaxRateDto ToDto(this IftaTaxRate entity);
}
