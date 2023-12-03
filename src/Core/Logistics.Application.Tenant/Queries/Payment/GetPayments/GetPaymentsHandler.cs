using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPaymentsHandler : RequestHandler<GetPaymentsQuery, PagedResponseResult<PaymentDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPaymentsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResponseResult<PaymentDto>> HandleValidated(
        GetPaymentsQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Payment>().CountAsync();
        var specification = new FilterPaymentsByInterval(req.OrderBy, req.StartDate, req.EndDate, req.Page, req.PageSize);
        
        var payments = _tenantUow.Repository<Payment>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return PagedResponseResult<PaymentDto>.Create(payments, totalItems, totalPages);
    }
}
