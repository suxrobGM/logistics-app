using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Financial.Tax;

[Mapper]
public static partial class TenantTaxRateMapper
{
    [MapperIgnoreSource(nameof(TenantTaxRate.Tenant))]
    public static partial TenantTaxRateDto ToDto(this TenantTaxRate entity);

    extension(IEnumerable<TenantTaxRate> entities)
    {
        public IEnumerable<TenantTaxRateDto> ToDto() =>
            entities.Select(ToDto);
    }
}
