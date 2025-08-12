using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentsHandler : IAppRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetPaymentsHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<PagedResult<PaymentDto>> Handle(
        GetPaymentsQuery req,
        CancellationToken ct)
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
