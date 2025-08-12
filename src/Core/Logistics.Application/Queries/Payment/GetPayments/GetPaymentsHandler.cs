using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentsHandler : RequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPaymentsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<PaymentDto>> HandleValidated(
        GetPaymentsQuery req,
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Payment>().CountAsync();

        var specification =
            new FilterPaymentsByInterval(req.OrderBy, req.StartDate, req.EndDate, req.Page, req.PageSize);

        var payments = _tenantUow.Repository<Payment>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<PaymentDto>.Succeed(payments, totalItems, req.PageSize);
    }
}
