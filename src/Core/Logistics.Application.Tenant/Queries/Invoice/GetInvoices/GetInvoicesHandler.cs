using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetInvoicesHandler : RequestHandler<GetInvoicesQuery, PagedResponseResult<InvoiceDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetInvoicesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResponseResult<InvoiceDto>> HandleValidated(
        GetInvoicesQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Invoice>().CountAsync();
        var specification = new FilterInvoicesByInterval(req.OrderBy, req.StartDate, req.EndDate, req.Page, req.PageSize, req.Descending);
        
        var invoicesDto = _tenantUow.Repository<Invoice>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return PagedResponseResult<InvoiceDto>.Create(invoicesDto, totalItems, totalPages);
    }
}
