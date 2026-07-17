using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DemoRequests.Queries;

internal sealed class GetDemoRequestsHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetDemoRequestsQuery, PagedResult<DemoRequestDto>>
{
    public Task<PagedResult<DemoRequestDto>> Handle(GetDemoRequestsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<DemoRequest>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(x =>
                x.Email.Contains(req.Search) ||
                x.FirstName.Contains(req.Search) ||
                x.LastName.Contains(req.Search) ||
                x.Company.Contains(req.Search));
        }

        var totalItems = query.Count();

        var items = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<DemoRequestDto>.Ok(items, totalItems, req.PageSize));
    }
}
