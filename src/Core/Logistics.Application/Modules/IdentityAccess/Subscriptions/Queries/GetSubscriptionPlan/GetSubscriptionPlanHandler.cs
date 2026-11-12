using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

internal sealed class
    GetSubscriptionPlanHandler(IMasterUnitOfWork masterUow) : IRequestHandler<GetSubscriptionPlanQuery, Result<SubscriptionPlanDto>>
{
    public async Task<Result<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlanQuery req, CancellationToken ct)
    {
        var entity = await masterUow.Repository<SubscriptionPlan>().GetAsync(i => i.Id == req.Id);

        if (entity is null)
        {
            return Result<SubscriptionPlanDto>.Fail($"Could not find a subscription plan with ID '{req.Id}'");
        }

        var dto = entity.ToDto();
        return Result<SubscriptionPlanDto>.Ok(dto);
    }
}
