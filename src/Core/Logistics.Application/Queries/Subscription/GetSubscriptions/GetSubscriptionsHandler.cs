using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionsHandler : RequestHandler<GetSubscriptionsQuery, PagedResult<SubscriptionDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetSubscriptionsHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResult<SubscriptionDto>> HandleValidated(
        GetSubscriptionsQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<Subscription>().CountAsync();
        var spec = new GetSubscriptions(req.OrderBy, req.Page, req.PageSize);

        var items = _masterUow.Repository<Subscription>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResult<SubscriptionDto>.Succeed(items, totalItems, req.PageSize);
    }
}
