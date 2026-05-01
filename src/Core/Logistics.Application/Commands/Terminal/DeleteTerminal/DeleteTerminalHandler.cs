using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteTerminalHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteTerminalCommand, Result>
{
    public async Task<Result> Handle(DeleteTerminalCommand req, CancellationToken ct)
    {
        var terminal = await tenantUow.Repository<Terminal>().GetByIdAsync(req.Id, ct);

        if (terminal is null)
        {
            return Result.Fail($"Could not find terminal with ID '{req.Id}'");
        }

        var assignedContainer = await tenantUow.Repository<Container>()
            .GetAsync(i => i.CurrentTerminalId == terminal.Id, ct);
        if (assignedContainer is not null)
        {
            return Result.Fail("Terminal has containers currently assigned and cannot be deleted");
        }

        var linkedLoad = await tenantUow.Repository<Load>()
            .GetAsync(i => i.OriginTerminalId == terminal.Id || i.DestinationTerminalId == terminal.Id, ct);
        if (linkedLoad is not null)
        {
            return Result.Fail("Terminal is referenced by one or more loads and cannot be deleted");
        }

        tenantUow.Repository<Terminal>().Delete(terminal);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
