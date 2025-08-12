using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionHandler : RequestHandler<GetSubscriptionQuery, Result<SubscriptionDto>>
{
    private readonly IMasterUnitOfWork _masterUow;

    public GetSubscriptionHandler(IMasterUnitOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    public override async Task<Result<SubscriptionDto>> Handle(
        GetSubscriptionQuery req, CancellationToken ct)
    {
        var subscription = await _masterUow.Repository<Subscription>().GetAsync(i => i.Id == req.Id);

        if (subscription is null)
        {
            return Result<SubscriptionDto>.Fail($"Could not find a subscription with ID '{req.Id}'");
        }

        var dto = subscription.ToDto();
        return Result<SubscriptionDto>.Succeed(dto);
    }
}
