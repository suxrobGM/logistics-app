using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPayrollsHandler(ITenantRepository tenantRepository) 
    : RequestHandler<GetPayrollsQuery, PagedResponseResult<PayrollDto>>
{
    protected override async Task<PagedResponseResult<PayrollDto>> HandleValidated(
        GetPayrollsQuery req, 
        CancellationToken cancellationToken)
    {
        int totalItems;
        BaseSpecification<Payroll> specification;

        if (!string.IsNullOrEmpty(req.EmployeeId))
        {
            specification = new GetEmployeePayrolls(req.EmployeeId, req.OrderBy, req.Descending);
            totalItems = await tenantRepository.CountAsync<Payroll>(i => i.EmployeeId == req.EmployeeId);
        }
        else
        {
            specification = new GetPayrolls(req.Search, req.OrderBy, req.Descending);
            totalItems = await tenantRepository.CountAsync<Payroll>();
        }
        
        var payrolls = tenantRepository.ApplySpecification(specification)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return PagedResponseResult<PayrollDto>.Create(payrolls, totalItems, totalPages);
    }
}
