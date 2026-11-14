using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.DemoRequests.Queries;

internal sealed class GetDemoRequestsHandler(
    IMasterUnitOfWork masterUow) : IRequestHandler<GetDemoRequestsQuery, PagedResult<DemoRequestDto>>
{
    private static readonly SortFields<DemoRequest> Sorts = new SortFields<DemoRequest>("-createdAt")
        .Add("firstName", x => x.FirstName)
        .Add("email", x => x.Email)
        .Add("company", x => x.Company)
        .Add("status", x => x.Status)
        .Add("createdAt", x => x.CreatedAt);

    public async Task<PagedResult<DemoRequestDto>> Handle(GetDemoRequestsQuery req, CancellationToken ct)
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

        return await query.ToPagedResultAsync(req, Sorts, i => i.ToDto(), ct);
    }
}
