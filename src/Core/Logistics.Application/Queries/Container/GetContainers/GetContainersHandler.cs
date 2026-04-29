using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetContainersHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetContainersQuery, PagedResult<ContainerDto>>
{
    public async Task<PagedResult<ContainerDto>> Handle(GetContainersQuery req, CancellationToken ct)
    {
        var totalItems = await tenantUow.Repository<Container>().CountAsync(ct: ct);
        var specification = new SearchContainers(
            req.Search,
            req.OrderBy,
            req.Page,
            req.PageSize,
            req.Status,
            req.IsoType,
            req.CurrentTerminalId);

        var containers = tenantUow.Repository<Container>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<ContainerDto>.Ok(containers, totalItems, req.PageSize);
    }
}
