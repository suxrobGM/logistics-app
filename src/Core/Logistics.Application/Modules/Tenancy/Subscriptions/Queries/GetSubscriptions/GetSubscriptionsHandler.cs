using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Subscriptions.Queries;

internal sealed class GetSubscriptionsHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetSubscriptionsQuery, PagedResult<SubscriptionDto>>
{
    private static readonly SortFields<Subscription> Sorts = new SortFields<Subscription>("-status")
        .Add("status", x => x.Status);

    public async Task<PagedResult<SubscriptionDto>> Handle(
        GetSubscriptionsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<Subscription>().Query();

        return await query.ToPagedResultAsync(req, Sorts, i => i.ToDto(), ct);
    }
}
