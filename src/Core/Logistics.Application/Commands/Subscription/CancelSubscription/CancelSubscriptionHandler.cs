using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CancelSubscriptionHandler : RequestHandler<CancelSubscriptionCommand, Result>
{
    private readonly ILogger<DeleteSubscriptionHandler> _logger;
    private readonly IMasterUnitOfWork _masterUow;
    private readonly IStripeService _stripeService;

    public CancelSubscriptionHandler(
        IMasterUnitOfWork masterUow,
        IStripeService stripeService,
        ILogger<DeleteSubscriptionHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        CancelSubscriptionCommand req, CancellationToken ct)
    {
        var subscription = await _masterUow.Repository<Subscription>().GetByIdAsync(req.Id);
        SubscriptionStatus? status = null;

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.Id}'");
        }

        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            _logger.LogInformation("Cancelling stripe subscription {StripeSubscriptionId}",
                subscription.StripeSubscriptionId);
            var stripeSubscription =
                await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId, false);
            status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        }

        subscription.Status = status ?? SubscriptionStatus.Cancelled;

        _masterUow.Repository<Subscription>().Update(subscription);
        await _masterUow.SaveChangesAsync();
        _logger.LogInformation("Cancelled subscription {SubscriptionId}", subscription.Id);
        return Result.Succeed();
    }
}
