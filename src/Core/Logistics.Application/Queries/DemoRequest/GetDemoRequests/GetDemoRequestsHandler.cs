using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDemoRequestsHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetDemoRequestsQuery, PagedResult<DemoRequestDto>>
{
    public async Task<PagedResult<DemoRequestDto>> Handle(GetDemoRequestsQuery req, CancellationToken ct)
    {
        var totalItems = await masterUow.Repository<DemoRequest>().CountAsync(null, ct);
        var spec = new GetDemoRequests(req.OrderBy, req.Page, req.PageSize, req.Search);

        var items = masterUow.Repository<DemoRequest>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<DemoRequestDto>.Succeed(items, totalItems, req.PageSize);
    }
}
