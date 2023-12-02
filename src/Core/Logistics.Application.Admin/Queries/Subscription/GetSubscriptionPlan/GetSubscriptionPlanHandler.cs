using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetSubscriptionPlanHandler : RequestHandler<GetSubscriptionPlanQuery, ResponseResult<SubscriptionPlanDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetSubscriptionPlanHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<ResponseResult<SubscriptionPlanDto>> HandleValidated(
        GetSubscriptionPlanQuery req, CancellationToken cancellationToken)
    {
        var entity = await _masterUow.Repository<SubscriptionPlan>().GetAsync(i => i.Id == req.Id);

        if (entity is null)
        {
            return ResponseResult<SubscriptionPlanDto>.CreateError($"Could not find a subscription plan with ID '{req.Id}'");
        }

        var dto = entity.ToDto();
        return ResponseResult<SubscriptionPlanDto>.CreateSuccess(dto);
    }
}
