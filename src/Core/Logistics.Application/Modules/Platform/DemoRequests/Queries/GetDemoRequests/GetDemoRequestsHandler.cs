using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DemoRequests.Queries;

internal sealed class GetDemoRequestsHandler(
    IMasterUnitOfWork masterUow) : IRequestHandler<GetDemoRequestsQuery, PagedResult<DemoRequestDto>>
{
    public Task<PagedResult<DemoRequestDto>> Handle(GetDemoRequestsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<DemoRequest>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(x =>
                x.Email.ToLower().Contains(search) ||
                x.FirstName.ToLower().Contains(search) ||
                x.LastName.ToLower().Contains(search) ||
                x.Company.ToLower().Contains(search));
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
