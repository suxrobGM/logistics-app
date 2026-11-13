using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Operations.Terminals;

[Mapper]
public static partial class TerminalMapper
{
    public static partial TerminalDto ToDto(this Terminal entity);
}
