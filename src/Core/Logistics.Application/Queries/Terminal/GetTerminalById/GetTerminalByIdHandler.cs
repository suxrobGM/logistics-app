using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTerminalByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTerminalByIdQuery, Result<TerminalDto>>
{
    public async Task<Result<TerminalDto>> Handle(GetTerminalByIdQuery req, CancellationToken ct)
    {
        var terminal = await tenantUow.Repository<Terminal>().GetByIdAsync(req.Id, ct);

        if (terminal is null)
        {
            return Result<TerminalDto>.Fail($"Could not find terminal with ID '{req.Id}'");
        }

        return Result<TerminalDto>.Ok(terminal.ToDto());
    }
}
