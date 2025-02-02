using Logistics.Application;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared;
using Logistics.Shared.Consts;

namespace Logistics.Application.Admin.Commands;

internal sealed class CreateSubscriptionHandler : RequestHandler<CreateSubscriptionCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;

    public CreateSubscriptionHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(
        CreateSubscriptionCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _masterUow.Repository<Domain.Entities.Tenant>().GetByIdAsync(req.TenantId);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.TenantId}'");
        }
        
        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.PlanId);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.TenantId}'");
        }

        var subscription = req.Status == SubscriptionStatus.Trial
            ? Subscription.Create30DaysTrial(tenant, subscriptionPlan)
            : Subscription.Create(tenant, subscriptionPlan);

        await _masterUow.Repository<Subscription>().AddAsync(subscription);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
