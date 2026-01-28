using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetCustomersHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery req, CancellationToken ct)
    {
        var totalItems = await tenantUow.Repository<Customer>().CountAsync(ct: ct);
        var specification = new SearchCustomers(req.Search, req.OrderBy, req.Page, req.PageSize);

        var customers = tenantUow.Repository<Customer>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<CustomerDto>.Succeed(customers, totalItems, req.PageSize);
    }
}
