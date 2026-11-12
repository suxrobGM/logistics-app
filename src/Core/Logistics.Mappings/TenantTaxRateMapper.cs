using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TenantTaxRateMapper
{
    [MapperIgnoreSource(nameof(TenantTaxRate.Tenant))]
    [MapperIgnoreSource(nameof(TenantTaxRate.DomainEvents))]
    [MapperIgnoreSource(nameof(TenantTaxRate.UpdatedAt))]
    [MapperIgnoreSource(nameof(TenantTaxRate.UpdatedBy))]
    [MapperIgnoreSource(nameof(TenantTaxRate.CreatedBy))]
    public static partial TenantTaxRateDto ToDto(this TenantTaxRate entity);

    extension(IEnumerable<TenantTaxRate> entities)
    {
        public IEnumerable<TenantTaxRateDto> ToDto() =>
            entities.Select(ToDto);
    }
}
