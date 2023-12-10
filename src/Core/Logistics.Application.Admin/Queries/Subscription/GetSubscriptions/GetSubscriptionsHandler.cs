using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetSubscriptionsHandler : RequestHandler<GetSubscriptionsQuery, PagedResponseResult<SubscriptionDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetSubscriptionsHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResponseResult<SubscriptionDto>> HandleValidated(
        GetSubscriptionsQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<Subscription>().CountAsync();
        var spec = new GetSubscriptions(req.OrderBy, req.Page, req.PageSize);

        var items = _masterUow.Repository<Subscription>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResponseResult<SubscriptionDto>.Create(items, totalItems, req.PageSize);
    }
}
