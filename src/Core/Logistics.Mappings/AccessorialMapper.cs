using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AccessorialMapper
{
    public static partial AccessorialChargeDto ToDto(this AccessorialCharge entity);

    [MapperIgnoreSource(nameof(LoadAccessorial.Load))]
    public static partial LoadAccessorialDto ToDto(this LoadAccessorial entity);
}
