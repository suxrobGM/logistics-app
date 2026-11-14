using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Subscriptions.Queries;

internal sealed class GetSubscriptionPlansHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetSubscriptionPlansQuery, PagedResult<SubscriptionPlanDto>>
{
    private static readonly SortFields<SubscriptionPlan> Sorts = new SortFields<SubscriptionPlan>("name")
        .Add("name", x => x.Name)
        .Add("tier", x => x.Tier)
        .Add("price", x => x.Price.Amount);

    public async Task<PagedResult<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlansQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<SubscriptionPlan>().Query();

        return await query.ToPagedResultAsync(req, Sorts, i => i.ToDto(), ct);
    }
}
