using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetCustomersHandler : RequestHandler<GetCustomersQuery, PagedResponseResult<CustomerDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetCustomersHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResponseResult<CustomerDto>> HandleValidated(
        GetCustomersQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Customer>().CountAsync();
        var specification = new SearchCustomers(req.Search, req.OrderBy, req.Page, req.PageSize);
        
        var customers = _tenantUow.Repository<Customer>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResponseResult<CustomerDto>.Create(customers, totalItems, req.PageSize);
    }
}
