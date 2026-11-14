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
    public async Task<PagedResult<PaymentDto>> Handle(
        GetPaymentsQuery req,
        CancellationToken ct)
    {
        var query = tenantUow.Repository<Payment>().Query()
            .Where(i => i.CreatedAt >= req.FromDate && i.CreatedAt <= req.ToDate);

        return await query.ToPagedResultAsync(req, i => i.ToDto(), ct);
    }
}
