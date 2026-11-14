using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Queries;

internal sealed class GetPaymentsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private static readonly SortFields<Payment> Sorts = new SortFields<Payment>("-createdAt")
        .Add("createdAt", x => x.CreatedAt)
        .Add("amount", x => x.Amount)
        .Add("status", x => x.Status)
        .Add("billingAddress", x => x.BillingAddress.Line1)
        .Add("description", x => x.Description);

    public async Task<PagedResult<PaymentDto>> Handle(
        GetPaymentsQuery req,
        CancellationToken ct)
    {
        var query = tenantUow.Repository<Payment>().Query();

        if (req.FromDate is { } fromDate)
        {
            query = query.Where(i => i.CreatedAt >= fromDate);
        }

        if (req.ToDate is { } toDate)
        {
            query = query.Where(i => i.CreatedAt <= toDate);
        }

        return await query.ToPagedResultAsync(req, Sorts, i => i.ToDto(), ct);
    }
}
