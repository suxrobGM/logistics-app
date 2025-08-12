using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionsHandler : RequestHandler<GetSubscriptionsQuery, PagedResult<SubscriptionDto>>
{
    private readonly IMasterUnitOfWork _masterUow;

    public GetSubscriptionsHandler(IMasterUnitOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    public override async Task<PagedResult<SubscriptionDto>> Handle(
        GetSubscriptionsQuery req, CancellationToken ct)
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
