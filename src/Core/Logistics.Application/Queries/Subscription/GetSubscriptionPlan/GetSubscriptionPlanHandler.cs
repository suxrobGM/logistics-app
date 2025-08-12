using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class
    GetSubscriptionPlanHandler : IAppRequestHandler<GetSubscriptionPlanQuery, Result<SubscriptionPlanDto>>
{
    private readonly IMasterUnitOfWork _masterUow;

    public GetSubscriptionPlanHandler(IMasterUnitOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    public async Task<Result<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlanQuery req, CancellationToken ct)
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
