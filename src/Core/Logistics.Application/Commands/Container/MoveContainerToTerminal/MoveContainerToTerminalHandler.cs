using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class MoveContainerToTerminalHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<MoveContainerToTerminalCommand, Result>
{
    public async Task<Result> Handle(MoveContainerToTerminalCommand req, CancellationToken ct)
    {
        var container = await tenantUow.Repository<Container>().GetByIdAsync(req.Id, ct);
        if (container is null)
        {
            return Result.Fail($"Could not find container with ID '{req.Id}'");
        }

        var terminal = await tenantUow.Repository<Terminal>().GetByIdAsync(req.TerminalId, ct);
        if (terminal is null)
        {
            return Result.Fail($"Could not find terminal with ID '{req.TerminalId}'");
        }

        container.MoveToTerminal(terminal);

        tenantUow.Repository<Container>().Update(container);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
