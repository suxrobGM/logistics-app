using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetInvoicesHandler : RequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetInvoicesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override Task<PagedResult<InvoiceDto>> HandleValidated(
        GetInvoicesQuery req, 
        CancellationToken cancellationToken)
    {
        if (req.LoadId.HasValue)
        {
            return GetLoadInvoices(req);
        }
        
        if (req.EmployeeId.HasValue || !string.IsNullOrEmpty(req.EmployeeName))
        {
            return GetPayrollInvoices(req);
        }
        
        return GetAllInvoices(req);
    }
    
    private async Task<PagedResult<InvoiceDto>> GetAllInvoices(GetInvoicesQuery req)
    {
        var totalItems = await _tenantUow.Repository<Invoice>().CountAsync();
        var invoicesDto = _tenantUow.Repository<Invoice>()
            .ApplySpecification(new GetInvoices(req.OrderBy, req.Page, req.PageSize))
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResult<InvoiceDto>.Succeed(invoicesDto, totalItems, req.PageSize);
    }

    private async Task<PagedResult<InvoiceDto>> GetLoadInvoices(GetInvoicesQuery req)
    {
        var totalItems = await _tenantUow.Repository<LoadInvoice>()
            .CountAsync(i => i.LoadId == req.LoadId);
        
        var invoicesDto = _tenantUow.Repository<LoadInvoice>()
            .ApplySpecification(new FilterInvoicesByLoadId(req.LoadId!.Value, req.OrderBy, req.Page, req.PageSize))
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResult<InvoiceDto>.Succeed(invoicesDto, totalItems, 1);
    }

    private async Task<PagedResult<InvoiceDto>> GetPayrollInvoices(GetInvoicesQuery req)
    {
        int totalItems;
        BaseSpecification<PayrollInvoice> specification;
        var repository = _tenantUow.Repository<PayrollInvoice>();

        if (req.EmployeeId.HasValue)
        {
            specification = new FilterInvoicesByEmployeeId(req.EmployeeId.Value, req.OrderBy, req.Page, req.PageSize);
            totalItems = await repository.CountAsync(i => i.EmployeeId == req.EmployeeId);
        }
        else
        {
            specification = new FilterInvoicesByEmployeeName(req.EmployeeName!, req.OrderBy, req.Page, req.PageSize);
            totalItems = await repository.CountAsync();
        }
        
        var invoice = repository.ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResult<InvoiceDto>.Succeed(invoice, totalItems, req.PageSize);
    }
}
