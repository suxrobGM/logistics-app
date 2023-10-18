using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetCustomersHandler : RequestHandler<GetCustomersQuery, PagedResponseResult<CustomerDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetCustomersHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<CustomerDto>> HandleValidated(
        GetCustomersQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Customer>().Count();
        var specification = new SearchCustomers(req.Search, req.OrderBy, req.Descending);
        
        var customers = _tenantRepository.ApplySpecification(specification)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(PagedResponseResult<CustomerDto>.Create(customers, totalItems, totalPages));
    }
}
