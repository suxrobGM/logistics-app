using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

internal sealed class GetSubscriptionsHandler(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<GetSubscriptionsQuery, PagedResult<SubscriptionDto>>
{
    public Task<PagedResult<SubscriptionDto>> Handle(
        GetSubscriptionsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<Subscription>().Query();

        var totalItems = query.Count();

        var items = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<SubscriptionDto>.Ok(items, totalItems, req.PageSize));
    }
}
