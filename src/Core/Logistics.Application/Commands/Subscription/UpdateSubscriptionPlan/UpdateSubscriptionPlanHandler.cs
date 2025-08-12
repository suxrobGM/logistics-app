using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UpdateSubscriptionPlanHandler : IAppRequestHandler<UpdateSubscriptionPlanCommand, Result>
{
    private readonly ILogger<UpdateSubscriptionPlanHandler> _logger;
    private readonly IMasterUnitOfWork _masterUow;
    private readonly IStripeService _stripeService;

    public UpdateSubscriptionPlanHandler(
        IMasterUnitOfWork masterUow,
        IStripeService stripeService,
        ILogger<UpdateSubscriptionPlanHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateSubscriptionPlanCommand req, CancellationToken ct)
    {
        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.Id);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.Id}'");
        }

        subscriptionPlan.Name = PropertyUpdater.UpdateIfChanged(req.Name, subscriptionPlan.Name);
        subscriptionPlan.Description = PropertyUpdater.UpdateIfChanged(req.Description, subscriptionPlan.Description);
        subscriptionPlan.Price = PropertyUpdater.UpdateIfChanged(req.Price, subscriptionPlan.Price.Amount);
        subscriptionPlan.TrialPeriod = PropertyUpdater.UpdateIfChanged(req.TrialPeriod, subscriptionPlan.TrialPeriod);
        subscriptionPlan.Interval = PropertyUpdater.UpdateIfChanged(req.Interval, subscriptionPlan.Interval);
        subscriptionPlan.IntervalCount =
            PropertyUpdater.UpdateIfChanged(req.IntervalCount, subscriptionPlan.IntervalCount);

        await _stripeService.UpdateSubscriptionPlanAsync(subscriptionPlan);
        _masterUow.Repository<SubscriptionPlan>().Update(subscriptionPlan);
        await _masterUow.SaveChangesAsync();
        _logger.LogInformation("Updated subscription plan {SubscriptionPlanId}", subscriptionPlan.Id);
        return Result.Succeed();
    }
}
