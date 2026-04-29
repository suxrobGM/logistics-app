using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTerminalsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTerminalsQuery, PagedResult<TerminalDto>>
{
    public async Task<PagedResult<TerminalDto>> Handle(GetTerminalsQuery req, CancellationToken ct)
    {
        var totalItems = await tenantUow.Repository<Terminal>().CountAsync(ct: ct);
        var specification = new SearchTerminals(
            req.Search,
            req.OrderBy,
            req.Page,
            req.PageSize,
            req.Type,
            req.CountryCode);

        var terminals = tenantUow.Repository<Terminal>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<TerminalDto>.Ok(terminals, totalItems, req.PageSize);
    }
}
