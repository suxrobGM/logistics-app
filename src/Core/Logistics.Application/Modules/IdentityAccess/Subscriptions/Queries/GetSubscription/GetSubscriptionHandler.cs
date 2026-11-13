using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

internal sealed class GetSubscriptionHandler(IMasterUnitOfWork masterUow) : IRequestHandler<GetSubscriptionQuery, Result<SubscriptionDto>>
{
    public async Task<Result<SubscriptionDto>> Handle(
        GetSubscriptionQuery req, CancellationToken ct)
    {
        var subscription = await masterUow.Repository<Subscription>().GetAsync(i => i.Id == req.Id);

        if (subscription is null)
        {
            return Result<SubscriptionDto>.Fail($"Could not find a subscription with ID '{req.Id}'", ErrorCodes.NotFound);
        }

        var dto = subscription.ToDto();
        return Result<SubscriptionDto>.Ok(dto);
    }
}
