using Logistics.Application;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared;

namespace Logistics.Application.Commands;

internal sealed class UpdateSubscriptionHandler : RequestHandler<UpdateSubscriptionCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;

    public UpdateSubscriptionHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateSubscriptionCommand req, CancellationToken cancellationToken)
    {
        var subscription = await _masterUow.Repository<Subscription>().GetByIdAsync(req.Id);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.TenantId}'");
        }

        if (!string.IsNullOrEmpty(req.TenantId) && subscription.TenantId != req.TenantId)
        {
            var tenant = await _masterUow.Repository<Domain.Entities.Tenant>().GetByIdAsync(req.TenantId);

            if (tenant is null)
            {
                return Result.Fail($"Could not find a tenant with ID '{req.TenantId}'");
            }

            subscription.Tenant = tenant;
        }
        
        if (!string.IsNullOrEmpty(req.PlanId) && subscription.PlanId != req.PlanId)
        {
            var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.PlanId);

            if (subscriptionPlan is null)
            {
                return Result.Fail($"Could not find a subscription plan with ID '{req.TenantId}'");
            }

            subscription.Plan = subscriptionPlan;
        }

        _masterUow.Repository<Subscription>().Update(subscription);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
