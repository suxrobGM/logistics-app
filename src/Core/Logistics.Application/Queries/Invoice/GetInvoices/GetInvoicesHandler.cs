using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetInvoicesHandler : RequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetInvoicesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<InvoiceDto>> HandleValidated(
        GetInvoicesQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Invoice>().CountAsync();
        var specification = new FilterInvoicesByInterval(req.OrderBy, req.StartDate, req.EndDate, req.Page, req.PageSize);
        
        var invoicesDto = _tenantUow.Repository<Invoice>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResult<InvoiceDto>.Succeed(invoicesDto, totalItems, req.PageSize);
    }
}
