using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class TerminalMapper
{
    public static partial TerminalDto ToDto(this Terminal entity);
}
