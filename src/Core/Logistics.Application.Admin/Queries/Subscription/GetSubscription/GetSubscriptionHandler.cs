using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetSubscriptionHandler : RequestHandler<GetSubscriptionQuery, ResponseResult<SubscriptionDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetSubscriptionHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<ResponseResult<SubscriptionDto>> HandleValidated(
        GetSubscriptionQuery req, CancellationToken cancellationToken)
    {
        var entity = await _masterUow.Repository<Subscription>().GetAsync(i => i.Id == req.Id);

        if (entity is null)
        {
            return ResponseResult<SubscriptionDto>.CreateError($"Could not find a subscription with ID '{req.Id}'");
        }

        var dto = entity.ToDto();
        return ResponseResult<SubscriptionDto>.CreateSuccess(dto);
    }
}
