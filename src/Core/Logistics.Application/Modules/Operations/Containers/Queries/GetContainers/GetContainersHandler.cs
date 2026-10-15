using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

internal sealed class GetContainersHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetContainersQuery, PagedResult<ContainerDto>>
{
    public async Task<PagedResult<ContainerDto>> Handle(GetContainersQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Container>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            // Number is stored canonical; the other two are free-form, so they keep the raw term.
            var number = Container.NormalizeNumber(req.Search);

            query = query.Where(i =>
                i.Number.Contains(number) ||
                (i.BookingReference != null && i.BookingReference.Contains(req.Search)) ||
                (i.BillOfLadingNumber != null && i.BillOfLadingNumber.Contains(req.Search)));
        }

        if (req.Status.HasValue)
        {
            query = query.Where(i => i.Status == req.Status.Value);
        }

        if (req.IsoType.HasValue)
        {
            query = query.Where(i => i.IsoType == req.IsoType.Value);
        }

        if (req.CurrentTerminalId.HasValue)
        {
            query = query.Where(i => i.CurrentTerminalId == req.CurrentTerminalId.Value);
        }

        var totalItems = query.Count();

        var containers = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .ToArray();

        // Both terminals in one query - reading them off the navigations costs two SELECTs per row.
        var terminalIds = containers.Select(c => c.CurrentTerminalId)
            .Concat(containers.Select(c => c.EmptyReturnTerminalId))
            .OfType<Guid>()
            .Distinct()
            .ToArray();

        var terminals = terminalIds.Length == 0
            ? []
            : await tenantUow.Repository<Terminal>().Query()
                .Where(t => terminalIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => new TerminalRef(t.Name, t.Code), ct);

        var now = DateTime.UtcNow;
        var items = containers
            .Select(c => c.ToDto(now, Find(terminals, c.CurrentTerminalId), Find(terminals, c.EmptyReturnTerminalId)))
            .ToArray();

        return PagedResult<ContainerDto>.Ok(items, totalItems, req.PageSize);
    }

    private static TerminalRef? Find(Dictionary<Guid, TerminalRef> terminals, Guid? id) =>
        id is { } key && terminals.TryGetValue(key, out var found) ? found : null;
}
