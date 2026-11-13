using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Modules.Financial.StripeConnect.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

internal sealed class RenewSubscriptionHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeSubscriptionService stripeSubscriptionService,
    ICurrentUserService currentUserService,
    ILogger<RenewSubscriptionHandler> logger) : IRequestHandler<RenewSubscriptionCommand, Result>
{
    public async Task<Result> Handle(
        RenewSubscriptionCommand req, CancellationToken ct)
    {
        var subscription = await masterUow.Repository<Subscription>().GetByIdAsync(req.Id, ct);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.Id}'", ErrorCodes.NotFound);
        }

        currentUserService.EnsureOwnsTenant(subscription.TenantId, "subscription");

        await tenantUow.SetCurrentTenantByIdAsync(subscription.TenantId);
        var truckCount = await tenantUow.Repository<Truck>().CountAsync(ct: ct);

        try
        {
            logger.LogInformation("Renewing stripe subscription {StripeSubscriptionId}",
                subscription.StripeSubscriptionId);
            var stripeSubscription =
                await stripeSubscriptionService.RenewSubscriptionAsync(subscription, subscription.Plan,
                    subscription.Tenant, truckCount);
            subscription.StripeSubscriptionId = stripeSubscription.Id;
            subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
            subscription.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;

            await masterUow.SaveChangesAsync(ct);
            logger.LogInformation("Renewed subscription {SubscriptionId}", subscription.Id);
            return Result.Ok();
        }
        catch (StripeApiException ex)
        {
            logger.LogError(ex, "Stripe error renewing subscription {SubscriptionId}", subscription.Id);

            var message = ex.Code switch
            {
                "resource_missing" => "No payment method on file. Please add a payment method in Manage Billing before resuming.",
                _ when ex.Message.Contains("payment source or default payment method") =>
                    "No payment method on file. Please add a payment method in Manage Billing before resuming.",
                _ => "Failed to resume subscription. Please try again."
            };

            return Result.Fail(message);
        }
    }
}
