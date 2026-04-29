using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class ContainerMapper
{
    [MapperIgnoreSource(nameof(Container.CurrentTerminal))]
    [MapperIgnoreSource(nameof(Container.DomainEvents))]
    private static partial ContainerDto ToDtoCore(this Container entity);

    public static ContainerDto ToDto(this Container entity)
    {
        var dto = entity.ToDtoCore();
        if (entity.CurrentTerminal is not null)
        {
            dto.CurrentTerminalName = entity.CurrentTerminal.Name;
            dto.CurrentTerminalCode = entity.CurrentTerminal.Code;
        }
        return dto;
    }
}
