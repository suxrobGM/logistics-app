using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Queries;

internal sealed class GetPaymentsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    public Task<PagedResult<PaymentDto>> Handle(
        GetPaymentsQuery req,
        CancellationToken ct)
    {
        var query = tenantUow.Repository<Payment>().Query()
            .Where(i => i.CreatedAt >= req.StartDate && i.CreatedAt <= req.EndDate);

        var totalItems = query.Count();

        var payments = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<PaymentDto>.Ok(payments, totalItems, req.PageSize));
    }
}
