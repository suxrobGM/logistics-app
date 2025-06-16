using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AddressMapper
{
    public static partial AddressDto ToDto(this Address entity);

    public static partial Address ToEntity(this AddressDto dto);
}
