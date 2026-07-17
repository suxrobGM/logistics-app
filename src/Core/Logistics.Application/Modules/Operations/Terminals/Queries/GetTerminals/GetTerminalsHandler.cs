using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Queries;

internal sealed class GetTerminalsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTerminalsQuery, PagedResult<TerminalDto>>
{
    public Task<PagedResult<TerminalDto>> Handle(GetTerminalsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Terminal>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(i => i.Name.Contains(req.Search) || i.Code.Contains(req.Search));
        }

        if (req.Type.HasValue)
        {
            query = query.Where(i => i.Type == req.Type.Value);
        }

        if (!string.IsNullOrEmpty(req.CountryCode))
        {
            query = query.Where(i => i.CountryCode == req.CountryCode);
        }

        var totalItems = query.Count();

        var terminals = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<TerminalDto>.Ok(terminals, totalItems, req.PageSize));
    }
}
