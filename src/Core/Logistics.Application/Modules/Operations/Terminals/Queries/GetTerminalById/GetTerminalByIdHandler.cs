using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Queries;

internal sealed class GetTerminalByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetTerminalByIdQuery, Terminal, TerminalDto>(tenantUow)
{
    protected override TerminalDto MapToDto(Terminal entity) => entity.ToDto();
}
