using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetCustomersHandler : RequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetCustomersHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<CustomerDto>> HandleValidated(
        GetCustomersQuery req, 
        CancellationToken cancellationToken)
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
