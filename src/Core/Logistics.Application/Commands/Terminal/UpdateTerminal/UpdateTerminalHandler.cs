using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTerminalHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateTerminalCommand, Result>
{
    public async Task<Result> Handle(UpdateTerminalCommand req, CancellationToken ct)
    {
        var terminal = await tenantUow.Repository<Terminal>().GetByIdAsync(req.Id, ct);
        if (terminal is null)
        {
            return Result.Fail($"Could not find terminal with ID '{req.Id}'");
        }

        if (!string.IsNullOrEmpty(req.Code) && req.Code != terminal.Code)
        {
            var conflict = await tenantUow.Repository<Terminal>().GetAsync(i => i.Code == req.Code, ct);
            if (conflict is not null && conflict.Id != terminal.Id)
            {
                return Result.Fail($"A terminal with code '{req.Code}' already exists");
            }
        }

        terminal.Name = PropertyUpdater.UpdateIfChanged(req.Name, terminal.Name);
        terminal.Code = PropertyUpdater.UpdateIfChanged(req.Code, terminal.Code);
        terminal.CountryCode = PropertyUpdater.UpdateIfChanged(req.CountryCode, terminal.CountryCode);
        terminal.Type = PropertyUpdater.UpdateIfChanged(req.Type, terminal.Type);
        terminal.Address = PropertyUpdater.UpdateIfChanged(req.Address, terminal.Address);
        terminal.Notes = PropertyUpdater.UpdateIfChanged(req.Notes, terminal.Notes);

        tenantUow.Repository<Terminal>().Update(terminal);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
