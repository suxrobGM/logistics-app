using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DemoRequests.Queries;

internal sealed class GetDemoRequestsHandler(
    IMasterUnitOfWork masterUow) : IRequestHandler<GetDemoRequestsQuery, PagedResult<DemoRequestDto>>
{
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

        return await query.ToPagedResultAsync(req, i => i.ToDto(), ct);
    }
}
