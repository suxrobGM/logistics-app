using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Domain.ValueObjects;
using Logistics.Shared;
using Logistics.Shared.Enums;

namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateSubscriptionHandler : RequestHandler<UpdateSubscriptionCommand, ResponseResult>
{
    private readonly IMasterUnityOfWork _masterUow;

    public UpdateSubscriptionHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateSubscriptionCommand req, CancellationToken cancellationToken)
    {
        var subscription = await _masterUow.Repository<Subscription>().GetByIdAsync(req.Id);

        if (subscription is null)
        {
            return ResponseResult.CreateError($"Could not find a subscription with ID '{req.TenantId}'");
        }

        if (!string.IsNullOrEmpty(req.TenantId) && subscription.TenantId != req.TenantId)
        {
            var tenant = await _masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId);

            if (tenant is null)
            {
                return ResponseResult.CreateError($"Could not find a tenant with ID '{req.TenantId}'");
            }

            subscription.Tenant = tenant;
        }
        
        if (!string.IsNullOrEmpty(req.PlanId) && subscription.PlanId != req.PlanId)
        {
            var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.PlanId);

            if (subscriptionPlan is null)
            {
                return ResponseResult.CreateError($"Could not find a subscription plan with ID '{req.TenantId}'");
            }

            subscription.Plan = subscriptionPlan;
        }

        _masterUow.Repository<Subscription>().Update(subscription);
        await _masterUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
