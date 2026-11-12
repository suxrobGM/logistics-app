using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

internal sealed class GetSubscriptionPlansHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetSubscriptionPlansQuery, PagedResult<SubscriptionPlanDto>>
{
    public async Task<PagedResult<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlansQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<SubscriptionPlan>().Query();

        return await query.ToPagedResultAsync(req, i => i.ToDto(), ct);
    }
}
