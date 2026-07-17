using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

internal sealed class GetContainersHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetContainersQuery, PagedResult<ContainerDto>>
{
    public Task<PagedResult<ContainerDto>> Handle(GetContainersQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Container>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(i =>
                i.Number.Contains(req.Search) ||
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
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<ContainerDto>.Ok(containers, totalItems, req.PageSize));
    }
}
