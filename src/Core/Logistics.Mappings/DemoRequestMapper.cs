using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class DemoRequestMapper
{
    [MapperIgnoreSource(nameof(DemoRequest.DomainEvents))]
    public static partial DemoRequestDto ToDto(this DemoRequest entity);
}
