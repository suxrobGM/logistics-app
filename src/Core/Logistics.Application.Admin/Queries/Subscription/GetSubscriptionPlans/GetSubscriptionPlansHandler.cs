using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetSubscriptionPlansHandler : RequestHandler<GetSubscriptionPlansQuery, PagedResponseResult<SubscriptionPlanDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetSubscriptionPlansHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResponseResult<SubscriptionPlanDto>> HandleValidated(
        GetSubscriptionPlansQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<SubscriptionPlan>().CountAsync();
        var spec = new GetSubscriptionPlans(req.OrderBy, req.Page, req.PageSize);

        var items = _masterUow.Repository<SubscriptionPlan>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto())
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return PagedResponseResult<SubscriptionPlanDto>.Create(items, totalItems, totalPages);
    }
}
