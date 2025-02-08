using Logistics.Application;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
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
        GetSubscriptionQuery req, CancellationToken cancellationToken)
    {
        var entity = await _masterUow.Repository<Subscription>().GetAsync(i => i.Id == req.Id);

        if (entity is null)
        {
            return Result<SubscriptionDto>.Fail($"Could not find a subscription with ID '{req.Id}'");
        }

        var dto = entity.ToDto();
        return Result<SubscriptionDto>.Succeed(dto);
    }
}
