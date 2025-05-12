using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPayrollInvoicesHandler : RequestHandler<GetPayrollInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPayrollInvoicesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }
    
    protected override async Task<PagedResult<InvoiceDto>> HandleValidated(
        GetPayrollInvoicesQuery req, 
        CancellationToken cancellationToken)
    {
        int totalItems;
        BaseSpecification<PayrollInvoice> specification;
        var payrollRepository = _tenantUow.Repository<PayrollInvoice>();

        if (req.EmployeeId.HasValue)
        {
            specification = new FilterPayrollsByEmployeeId(req.EmployeeId.Value, req.OrderBy, req.Page, req.PageSize);
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
        
        return PagedResult<InvoiceDto>.Succeed(payrolls, totalItems, req.PageSize);
    }
}
