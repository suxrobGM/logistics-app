using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Domain.ValueObjects;
using Logistics.Shared;
using Logistics.Shared.Enums;

namespace Logistics.Application.Admin.Commands;

internal sealed class CreateSubscriptionHandler : RequestHandler<CreateSubscriptionCommand, ResponseResult>
{
    private readonly IMasterUnityOfWork _masterUow;

    public CreateSubscriptionHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreateSubscriptionCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId);

        if (tenant is null)
        {
            return ResponseResult.CreateError($"Could not find a tenant with ID '{req.TenantId}'");
        }
        
        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.PlanId);

        if (subscriptionPlan is null)
        {
            return ResponseResult.CreateError($"Could not find a subscription plan with ID '{req.TenantId}'");
        }

        var subscription = req.Status == SubscriptionStatus.Trial
            ? Subscription.Create30DaysTrial(tenant, subscriptionPlan)
            : Subscription.Create(tenant, subscriptionPlan);

        await _masterUow.Repository<Subscription>().AddAsync(subscription);
        await _masterUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
