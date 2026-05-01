using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateSubscriptionPlanHandler(
    IMasterUnitOfWork masterUow,
    IStripePlanService stripePlanService,
    ILogger<CreateSubscriptionPlanHandler> logger) : IAppRequestHandler<CreateSubscriptionPlanCommand, Result>
{
    public async Task<Result> Handle(
        CreateSubscriptionPlanCommand req, CancellationToken ct)
    {
        var subscriptionPlan = new SubscriptionPlan
        {
            Name = req.Name,
            Description = req.Description,
            Price = new() { Amount = req.Price, Currency = "USD" },
            Interval = req.Interval,
            IntervalCount = req.IntervalCount,
            Tier = req.Tier,
            PerTruckPrice = new() { Amount = req.PerTruckPrice, Currency = "USD" },
            MaxTrucks = req.MaxTrucks,
            WeeklyAiRequestQuota = req.WeeklyAiRequestQuota,
            AllowedModelTier = req.AllowedModelTier
        };

        var result = await stripePlanService.CreatePlanAsync(subscriptionPlan);

        subscriptionPlan.StripeProductId = result.Product.Id;
        subscriptionPlan.StripePriceId = result.BasePrice.Id;
        subscriptionPlan.StripePerTruckPriceId = result.PerTruckPrice.Id;
        subscriptionPlan.StripeAiOveragePriceId = result.AiOveragePrice?.Id;
        await masterUow.Repository<SubscriptionPlan>().AddAsync(subscriptionPlan, ct);
        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
