using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeleteSubscriptionHandler : RequestHandler<DeleteSubscriptionCommand, Result>
{
    private readonly ILogger<DeleteSubscriptionHandler> _logger;
    private readonly IMasterUnitOfWork _masterUow;
    private readonly IStripeService _stripeService;

    public DeleteSubscriptionHandler(
        IMasterUnitOfWork masterUow,
        IStripeService stripeService,
        ILogger<DeleteSubscriptionHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        DeleteSubscriptionCommand req, CancellationToken ct)
    {
        var subscription = await _masterUow.Repository<Subscription>().GetByIdAsync(req.Id);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with ID '{req.Id}'");
        }

        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            _logger.LogInformation("Cancelling stripe subscription {StripeSubscriptionId}",
                subscription.StripeSubscriptionId);
            await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
        }

        _masterUow.Repository<Subscription>().Delete(subscription);
        await _masterUow.SaveChangesAsync();
        _logger.LogInformation("Deleted subscription {SubscriptionId}", subscription.Id);
        return Result.Succeed();
    }
}
