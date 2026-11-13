using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Queries;

internal sealed class GetTerminalsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTerminalsQuery, PagedResult<TerminalDto>>
{
    public async Task<PagedResult<TerminalDto>> Handle(GetTerminalsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Terminal>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            // Code is stored canonical; Name is free-form, so it matches case-insensitively.
            var code = Terminal.NormalizeCode(req.Search);
            var search = req.Search.ToLower();

            query = query.Where(i => i.Name.ToLower().Contains(search) || i.Code.Contains(code));
        }

        if (req.Type.HasValue)
        {
            query = query.Where(i => i.Type == req.Type.Value);
        }

        if (!string.IsNullOrEmpty(req.CountryCode))
        {
            query = query.Where(i => i.Address.Country == req.CountryCode);
        }

        return await query.ToPagedResultAsync(req, i => i.ToDto(), ct);
    }
}
