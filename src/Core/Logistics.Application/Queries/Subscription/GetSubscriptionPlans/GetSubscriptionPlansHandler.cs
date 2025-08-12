using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class
    GetSubscriptionPlansHandler : RequestHandler<GetSubscriptionPlansQuery, PagedResult<SubscriptionPlanDto>>
{
    private readonly IMasterUnitOfWork _masterUow;

    public GetSubscriptionPlansHandler(IMasterUnitOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    public override async Task<PagedResult<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlansQuery req, CancellationToken ct)
    {
        var totalItems = await _masterUow.Repository<SubscriptionPlan>().CountAsync();
        var spec = new GetSubscriptionPlans(req.OrderBy, req.Page, req.PageSize);

        var items = _masterUow.Repository<SubscriptionPlan>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<SubscriptionPlanDto>.Succeed(items, totalItems, req.PageSize);
    }
}
