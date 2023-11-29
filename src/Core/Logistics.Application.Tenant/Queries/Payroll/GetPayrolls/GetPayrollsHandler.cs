using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPayrollsHandler
    : RequestHandler<GetPayrollsQuery, PagedResponseResult<PayrollDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPayrollsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }
    
    protected override async Task<PagedResponseResult<PayrollDto>> HandleValidated(
        GetPayrollsQuery req, 
        CancellationToken cancellationToken)
    {
        int totalItems;
        BaseSpecification<Payroll> specification;
        var payrollRepository = _tenantUow.Repository<Payroll>();

        if (!string.IsNullOrEmpty(req.EmployeeId))
        {
            specification = new FilterPayrollsByEmployeeId(req.EmployeeId, req.OrderBy, req.Page, req.PageSize, req.Descending);
            totalItems = await payrollRepository.CountAsync(i => i.EmployeeId == req.EmployeeId);
        }
        else
        {
            specification = new GetPayrolls(req.Search, req.OrderBy, req.Page, req.PageSize, req.Descending);
            totalItems = await payrollRepository.CountAsync();
        }
        
        var payrolls = payrollRepository.ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return PagedResponseResult<PayrollDto>.Create(payrolls, totalItems, totalPages);
    }
}
