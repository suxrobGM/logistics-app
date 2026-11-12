using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Queries;

internal sealed class GetCustomersHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Customer>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(search));
        }

        return await query.ToPagedResultAsync(req, i => i.ToDto(), ct);
    }
}
