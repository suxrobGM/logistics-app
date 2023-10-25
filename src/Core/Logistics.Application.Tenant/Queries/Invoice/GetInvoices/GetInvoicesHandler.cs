using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetInvoicesHandler : RequestHandler<GetInvoicesQuery, PagedResponseResult<InvoiceDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetInvoicesHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<InvoiceDto>> HandleValidated(
        GetInvoicesQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Invoice>().Count();
        var specification = new FilterInvoicesByInterval(req.OrderBy, req.StartDate, req.EndDate, req.Descending);
        
        var invoicesDto = _tenantRepository.ApplySpecification(specification)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(PagedResponseResult<InvoiceDto>.Create(invoicesDto, totalItems, totalPages));
    }
}
