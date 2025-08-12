using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionHandler : RequestHandler<GetSubscriptionQuery, Result<SubscriptionDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetSubscriptionHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result<SubscriptionDto>> HandleValidated(
        GetSubscriptionQuery req, CancellationToken ct)
    {
        var subscription = await _masterUow.Repository<Subscription>().GetAsync(i => i.Id == req.Id);

        if (subscription is null)
            return Result<SubscriptionDto>.Fail($"Could not find a subscription with ID '{req.Id}'");

        var dto = subscription.ToDto();
        return Result<SubscriptionDto>.Succeed(dto);
    }
}
