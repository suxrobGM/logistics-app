using Logistics.Application.Modules.Financial.StripeConnect.Services;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

internal sealed class CancelSubscriptionHandler(
    IMasterUnitOfWork masterUow,
    IStripeSubscriptionService stripeSubscriptionService,
    ILogger<DeleteSubscriptionHandler> logger) : IAppRequestHandler<CancelSubscriptionCommand, Result>
{
    public async Task<Result> Handle(
        CancelSubscriptionCommand req, CancellationToken ct)
    {
        var subscription = await masterUow.Repository<Subscription>().GetByIdAsync(req.Id, ct);
        SubscriptionStatus? status = null;

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.Id}'");
        }

        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            logger.LogInformation("Cancelling stripe subscription {StripeSubscriptionId}",
                subscription.StripeSubscriptionId);
            var stripeSubscription =
                await stripeSubscriptionService.CancelSubscriptionAsync(subscription.StripeSubscriptionId, false);
            status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
            subscription.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;
        }

        subscription.Status = status ?? SubscriptionStatus.Cancelled;

        masterUow.Repository<Subscription>().Update(subscription);
        await masterUow.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled subscription {SubscriptionId}", subscription.Id);
        return Result.Ok();
    }
}
