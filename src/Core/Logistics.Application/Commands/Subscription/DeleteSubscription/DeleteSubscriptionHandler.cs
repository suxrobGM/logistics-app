using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeleteSubscriptionHandler(
    IMasterUnitOfWork masterUow,
    IStripeSubscriptionService stripeSubscriptionService,
    ILogger<DeleteSubscriptionHandler> logger) : IAppRequestHandler<DeleteSubscriptionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSubscriptionCommand req, CancellationToken ct)
    {
        var subscription = await masterUow.Repository<Subscription>().GetByIdAsync(req.Id, ct);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.Id}'");
        }

        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            logger.LogInformation("Cancelling stripe subscription {StripeSubscriptionId}",
                subscription.StripeSubscriptionId);
            await stripeSubscriptionService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
        }

        masterUow.Repository<Subscription>().Delete(subscription);
        await masterUow.SaveChangesAsync(ct);
        logger.LogInformation("Deleted subscription {SubscriptionId}", subscription.Id);
        return Result.Ok();
    }
}
