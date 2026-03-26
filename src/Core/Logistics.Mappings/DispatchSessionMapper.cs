using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class DispatchSessionMapper
{
    [MapperIgnoreSource(nameof(DispatchSession.DomainEvents))]
    [MapperIgnoreSource(nameof(DispatchSession.Decisions))]
    public static partial DispatchSessionDto ToDto(this DispatchSession entity);

    public static DispatchSessionDto ToDtoWithDecisions(this DispatchSession entity)
    {
        var dto = entity.ToDto();
        dto.Decisions = entity.Decisions.Select(d => d.ToDto()).ToList();
        return dto;
    }
}
