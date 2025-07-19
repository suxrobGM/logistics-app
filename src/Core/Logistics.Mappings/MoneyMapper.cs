using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class MoneyMapper
{
    public static partial MoneyDto ToDto(this Money entity);
    public static partial Money ToEntity(this MoneyDto dto);
}
