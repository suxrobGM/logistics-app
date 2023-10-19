using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPaymentsHandler : RequestHandler<GetPaymentsQuery, PagedResponseResult<PaymentDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetPaymentsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<PaymentDto>> HandleValidated(
        GetPaymentsQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Payment>().Count();
        var specification = new FilterPaymentsByInterval(req.OrderBy, req.StartDate, req.EndDate, req.Descending);
        
        var payments = _tenantRepository.ApplySpecification(specification)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(PagedResponseResult<PaymentDto>.Create(payments, totalItems, totalPages));
    }
}
