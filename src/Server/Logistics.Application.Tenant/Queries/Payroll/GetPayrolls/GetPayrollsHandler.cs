using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPayrollsHandler
    : RequestHandler<GetPayrollsQuery, PagedResult<PayrollDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPayrollsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }
    
    protected override async Task<PagedResult<PayrollDto>> HandleValidated(
        GetPayrollsQuery req, 
        CancellationToken cancellationToken)
    {
        int totalItems;
        BaseSpecification<Payroll> specification;
        var payrollRepository = _tenantUow.Repository<Payroll>();

        if (!string.IsNullOrEmpty(req.EmployeeId))
        {
            specification = new FilterPayrollsByEmployeeId(req.EmployeeId, req.OrderBy, req.Page, req.PageSize);
            totalItems = await payrollRepository.CountAsync(i => i.EmployeeId == req.EmployeeId);
        }
        else
        {
            specification = new GetPayrolls(req.Search, req.OrderBy, req.Page, req.PageSize);
            totalItems = await payrollRepository.CountAsync();
        }
        
        var payrolls = payrollRepository.ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResult<PayrollDto>.Succeed(payrolls, totalItems, req.PageSize);
    }
}
