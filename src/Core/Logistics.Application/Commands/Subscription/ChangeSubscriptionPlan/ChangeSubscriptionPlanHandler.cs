using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class ChangeSubscriptionPlanHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeSubscriptionService stripeSubscriptionService,
    ILogger<ChangeSubscriptionPlanHandler> logger) : IAppRequestHandler<ChangeSubscriptionPlanCommand, Result>
{
    public async Task<Result> Handle(
        ChangeSubscriptionPlanCommand req, CancellationToken ct)
    {
        var subscription = await masterUow.Repository<Subscription>().GetByIdAsync(req.SubscriptionId, ct);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.SubscriptionId}'");
        }

        if (subscription.Status is not (SubscriptionStatus.Active or SubscriptionStatus.Trialing))
        {
            return Result.Fail(
                $"Cannot change plan for subscription with status '{subscription.Status}'. Subscription must be active or trialing.");
        }

        if (subscription.PlanId == req.NewPlanId)
        {
            return Result.Fail("The subscription is already on this plan.");
        }

        var newPlan = await masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.NewPlanId, ct);

        if (newPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.NewPlanId}'");
        }

        if (string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            return Result.Fail("Subscription does not have a Stripe subscription ID.");
        }

        await tenantUow.SetCurrentTenantByIdAsync(subscription.TenantId);
        var truckCount = await tenantUow.Repository<Truck>().CountAsync(ct: ct);

        logger.LogInformation(
            "Changing subscription {SubscriptionId} from plan {OldPlanId} to {NewPlanId}",
            subscription.Id, subscription.PlanId, req.NewPlanId);

        var stripeSubscription = await stripeSubscriptionService.ChangeSubscriptionPlanAsync(
            subscription.StripeSubscriptionId, newPlan, truckCount);

        subscription.PlanId = newPlan.Id;
        subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        subscription.StartDate = stripeSubscription.StartDate;
        subscription.NextBillingDate = stripeSubscription.Items.Data.First().CurrentPeriodEnd;
        subscription.TrialEndDate = stripeSubscription.TrialEnd;

        masterUow.Repository<Subscription>().Update(subscription);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Changed subscription {SubscriptionId} to plan {NewPlanId}, truck count: {TruckCount}",
            subscription.Id, newPlan.Id, truckCount);
        return Result.Ok();
    }
}
