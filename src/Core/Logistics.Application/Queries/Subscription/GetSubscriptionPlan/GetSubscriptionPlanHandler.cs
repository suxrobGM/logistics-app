using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionPlanHandler : RequestHandler<GetSubscriptionPlanQuery, Result<SubscriptionPlanDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetSubscriptionPlanHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result<SubscriptionPlanDto>> HandleValidated(
        GetSubscriptionPlanQuery req, CancellationToken cancellationToken)
    {
        var entity = await _masterUow.Repository<SubscriptionPlan>().GetAsync(i => i.Id == req.Id);

        if (entity is null)
        {
            return Result<SubscriptionPlanDto>.Fail($"Could not find a subscription plan with ID '{req.Id}'");
        }

        var dto = entity.ToDto();
        return Result<SubscriptionPlanDto>.Succeed(dto);
    }
}
