using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AccessorialMapper
{
    [MapperIgnoreSource(nameof(AccessorialCharge.DomainEvents))]
    public static partial AccessorialChargeDto ToDto(this AccessorialCharge entity);

    [MapperIgnoreSource(nameof(LoadAccessorial.DomainEvents))]
    [MapperIgnoreSource(nameof(LoadAccessorial.Load))]
    [MapperIgnoreSource(nameof(LoadAccessorial.LoadId))]
    public static partial LoadAccessorialDto ToDto(this LoadAccessorial entity);
}
