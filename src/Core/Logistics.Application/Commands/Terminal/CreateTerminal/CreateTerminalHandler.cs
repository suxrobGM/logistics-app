using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateTerminalHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateTerminalCommand, Result<TerminalDto>>
{
    public async Task<Result<TerminalDto>> Handle(CreateTerminalCommand req, CancellationToken ct)
    {
        var existing = await tenantUow.Repository<Terminal>().GetAsync(i => i.Code == req.Code, ct);
        if (existing is not null)
        {
            return Result<TerminalDto>.Fail($"A terminal with code '{req.Code}' already exists");
        }

        var terminal = new Terminal
        {
            Name = req.Name,
            Code = req.Code,
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
