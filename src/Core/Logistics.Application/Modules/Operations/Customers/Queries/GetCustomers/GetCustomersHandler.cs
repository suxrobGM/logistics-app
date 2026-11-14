using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Customers.Queries;

internal sealed class GetCustomersHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    private static readonly SortFields<Customer> Sorts = new SortFields<Customer>("name")
        .Add("name", x => x.Name)
        .Add("email", x => x.Email)
        .Add("phone", x => x.Phone)
        .Add("createdAt", x => x.CreatedAt);

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Customer>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(search));
        }

        return await query.ToPagedResultAsync(req, Sorts, i => i.ToDto(), ct);
    }
}
