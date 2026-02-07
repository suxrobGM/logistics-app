using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class RenewSubscriptionHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeSubscriptionService stripeSubscriptionService,
    ILogger<RenewSubscriptionHandler> logger) : IAppRequestHandler<RenewSubscriptionCommand, Result>
{
    public async Task<Result> Handle(
        RenewSubscriptionCommand req, CancellationToken ct)
    {
        var subscription = await masterUow.Repository<Subscription>().GetByIdAsync(req.Id, ct);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.Id}'");
        }

        await tenantUow.SetCurrentTenantByIdAsync(subscription.TenantId);
        var truckCount = await tenantUow.Repository<Truck>().CountAsync(ct: ct);

        logger.LogInformation("Renewing stripe subscription {StripeSubscriptionId}",
            subscription.StripeSubscriptionId);
        var stripeSubscription =
            await stripeSubscriptionService.RenewSubscriptionAsync(subscription, subscription.Plan, subscription.Tenant,
                truckCount);
        subscription.StripeSubscriptionId = stripeSubscription.Id;
        subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        subscription.StartDate = stripeSubscription.StartDate;
        subscription.NextBillingDate = stripeSubscription.Items.Data.First().CurrentPeriodEnd;
        subscription.TrialEndDate = stripeSubscription.TrialEnd;

        masterUow.Repository<Subscription>().Update(subscription);
        await masterUow.SaveChangesAsync(ct);
        logger.LogInformation("Renewed subscription {SubscriptionId}", subscription.Id);
        return Result.Ok();
    }
}
