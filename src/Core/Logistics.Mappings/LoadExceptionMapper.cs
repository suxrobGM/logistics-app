using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class LoadExceptionMapper
{
    [MapperIgnoreSource(nameof(LoadException.Load))]
    public static partial LoadExceptionDto ToDto(this LoadException entity);
}
