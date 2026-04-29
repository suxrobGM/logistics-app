using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TerminalMapper
{
    [MapperIgnoreSource(nameof(Terminal.DomainEvents))]
    public static partial TerminalDto ToDto(this Terminal entity);
}
