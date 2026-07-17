using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

internal sealed class GetSubscriptionPlansHandler(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<GetSubscriptionPlansQuery, PagedResult<SubscriptionPlanDto>>
{
    public Task<PagedResult<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlansQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<SubscriptionPlan>().Query();

        var totalItems = query.Count();

        var items = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<SubscriptionPlanDto>.Ok(items, totalItems, req.PageSize));
    }
}
