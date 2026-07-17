using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Queries;

internal sealed class GetCustomersHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    public Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Customer>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(i => i.Name.Contains(req.Search));
        }

        var totalItems = query.Count();

        var customers = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<CustomerDto>.Ok(customers, totalItems, req.PageSize));
    }
}
