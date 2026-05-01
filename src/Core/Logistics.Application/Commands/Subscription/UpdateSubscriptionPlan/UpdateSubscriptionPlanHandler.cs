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
    IStripePlanService stripePlanService,
    ILogger<UpdateSubscriptionPlanHandler> logger) : IAppRequestHandler<UpdateSubscriptionPlanCommand, Result>
{

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
        if (req.Price is not null && req.Price != subscriptionPlan.Price.Amount)
        {
            subscriptionPlan.Price = new() { Amount = req.Price.Value, Currency = subscriptionPlan.Price.Currency };
        }
        if (req.PerTruckPrice is not null && req.PerTruckPrice != subscriptionPlan.PerTruckPrice.Amount)
        {
            subscriptionPlan.PerTruckPrice = new() { Amount = req.PerTruckPrice.Value, Currency = subscriptionPlan.PerTruckPrice.Currency };
        }
        subscriptionPlan.Interval = PropertyUpdater.UpdateIfChanged(req.Interval, subscriptionPlan.Interval);
        subscriptionPlan.IntervalCount =
            PropertyUpdater.UpdateIfChanged(req.IntervalCount, subscriptionPlan.IntervalCount);
        if (req.MaxTrucks.HasValue)
        {
            subscriptionPlan.MaxTrucks = req.MaxTrucks;
        }
        if (req.WeeklyAiRequestQuota.HasValue)
        {
            subscriptionPlan.WeeklyAiRequestQuota = req.WeeklyAiRequestQuota;
        }
        subscriptionPlan.Tier = PropertyUpdater.UpdateIfChanged(req.Tier, subscriptionPlan.Tier);
        subscriptionPlan.AllowedModelTier = PropertyUpdater.UpdateIfChanged(req.AllowedModelTier, subscriptionPlan.AllowedModelTier);

        var result = await stripePlanService.UpdatePlanAsync(subscriptionPlan);
        subscriptionPlan.StripePriceId = result.BasePrice.Id;
        subscriptionPlan.StripePerTruckPriceId = result.PerTruckPrice.Id;
        masterUow.Repository<SubscriptionPlan>().Update(subscriptionPlan);
        await masterUow.SaveChangesAsync(ct);
        logger.LogInformation("Updated subscription plan {SubscriptionPlanId}", subscriptionPlan.Id);
        return Result.Ok();
    }
}
