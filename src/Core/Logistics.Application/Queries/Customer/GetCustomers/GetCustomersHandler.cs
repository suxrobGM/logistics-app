using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetCustomersHandler : RequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetCustomersHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<CustomerDto>> HandleValidated(
        GetCustomersQuery req,
        CancellationToken ct)
    {
        var totalItems = await _tenantUow.Repository<Customer>().CountAsync();
        var specification = new SearchCustomers(req.Search, req.OrderBy, req.Page, req.PageSize);

        var customers = _tenantUow.Repository<Customer>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<CustomerDto>.Succeed(customers, totalItems, req.PageSize);
    }
}
