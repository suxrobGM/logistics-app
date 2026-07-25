using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Commands;

internal sealed class CreateTerminalHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateTerminalCommand, Result<TerminalDto>>
{
    public async Task<Result<TerminalDto>> Handle(CreateTerminalCommand req, CancellationToken ct)
    {
        // Stored codes are canonical, so the search term must be too, or "uslax" reads as free
        // and then collides on insert.
        var code = Terminal.NormalizeCode(req.Code);
        var existing = await tenantUow.Repository<Terminal>().GetAsync(i => i.Code == code, ct);
        if (existing is not null)
        {
            return Result<TerminalDto>.Fail($"A terminal with code '{code}' already exists");
        }

        var terminal = new Terminal
        {
            Name = req.Name,
            Code = code,
            CountryCode = req.CountryCode,
            Type = req.Type,
            Address = req.Address,
            Notes = req.Notes
        };

        await tenantUow.Repository<Terminal>().AddAsync(terminal, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result<TerminalDto>.Ok(terminal.ToDto());
    }
}
