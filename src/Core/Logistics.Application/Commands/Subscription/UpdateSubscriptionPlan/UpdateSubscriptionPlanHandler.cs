using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UpdateSubscriptionPlanHandler(
    IMasterUnitOfWork masterUow,
    IStripeSubscriptionService stripeSubscriptionService,
    ILogger<UpdateSubscriptionPlanHandler> logger) : IAppRequestHandler<UpdateSubscriptionPlanCommand, Result>
{
    private readonly ILogger<UpdateSubscriptionPlanHandler> logger = logger;
    private readonly IMasterUnitOfWork masterUow = masterUow;
    private readonly IStripeSubscriptionService stripeSubscriptionService = stripeSubscriptionService;

    public async Task<Result> Handle(
        UpdateSubscriptionPlanCommand req, CancellationToken ct)
    {
        var subscriptionPlan = await masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.Id, ct);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.Id}'");
        }

        subscriptionPlan.Name = PropertyUpdater.UpdateIfChanged(req.Name, subscriptionPlan.Name);
        subscriptionPlan.Description = PropertyUpdater.UpdateIfChanged(req.Description, subscriptionPlan.Description);
        subscriptionPlan.Price = PropertyUpdater.UpdateIfChanged(req.Price, subscriptionPlan.Price.Amount);
        subscriptionPlan.PerTruckPrice = PropertyUpdater.UpdateIfChanged(req.PerTruckPrice, subscriptionPlan.PerTruckPrice.Amount);
        subscriptionPlan.TrialPeriod = PropertyUpdater.UpdateIfChanged(req.TrialPeriod, subscriptionPlan.TrialPeriod);
        subscriptionPlan.Interval = PropertyUpdater.UpdateIfChanged(req.Interval, subscriptionPlan.Interval);
        subscriptionPlan.IntervalCount =
            PropertyUpdater.UpdateIfChanged(req.IntervalCount, subscriptionPlan.IntervalCount);
        if (req.MaxTrucks.HasValue)
        {
            subscriptionPlan.MaxTrucks = req.MaxTrucks;
        }
        subscriptionPlan.AnnualDiscountPercent = PropertyUpdater.UpdateIfChanged(req.AnnualDiscountPercent, subscriptionPlan.AnnualDiscountPercent);
        subscriptionPlan.Tier = PropertyUpdater.UpdateIfChanged(req.Tier, subscriptionPlan.Tier);

        var (product, activeBasePrice, activePerTruckPrice) = await stripeSubscriptionService.UpdateSubscriptionPlanAsync(subscriptionPlan);
        subscriptionPlan.StripePriceId = activeBasePrice.Id;
        subscriptionPlan.StripePerTruckPriceId = activePerTruckPrice.Id;
        masterUow.Repository<SubscriptionPlan>().Update(subscriptionPlan);
        await masterUow.SaveChangesAsync(ct);
        logger.LogInformation("Updated subscription plan {SubscriptionPlanId}", subscriptionPlan.Id);
        return Result.Ok();
    }
}
