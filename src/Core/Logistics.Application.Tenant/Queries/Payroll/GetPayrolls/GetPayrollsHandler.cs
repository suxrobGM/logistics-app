using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPayrollsHandler : RequestHandler<GetPayrollsQuery, PagedResponseResult<PayrollDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetPayrollsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<PayrollDto>> HandleValidated(
        GetPayrollsQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Payroll>().Count();
        var specification = new GetPayrolls(req.OrderBy, req.Descending);
        
        var payrolls = _tenantRepository.ApplySpecification(specification)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(PagedResponseResult<PayrollDto>.Create(payrolls, totalItems, totalPages));
    }
}
