using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateSubscriptionPlanHandler(
    IMasterUnitOfWork masterUow,
    IStripeSubscriptionService stripeSubscriptionService,
    ILogger<CreateSubscriptionPlanHandler> logger) : IAppRequestHandler<CreateSubscriptionPlanCommand, Result>
{
    public async Task<Result> Handle(
        CreateSubscriptionPlanCommand req, CancellationToken ct)
    {
        var subscriptionPlan = new SubscriptionPlan
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            TrialPeriod = req.TrialPeriod,
            Interval = req.Interval,
            IntervalCount = req.IntervalCount,
            Tier = req.Tier,
            PerTruckPrice = req.PerTruckPrice,
            MaxTrucks = req.MaxTrucks,
            AnnualDiscountPercent = req.AnnualDiscountPercent
        };

        var (product, basePrice, perTruckPrice) = await stripeSubscriptionService.CreateSubscriptionPlanAsync(subscriptionPlan);

        subscriptionPlan.StripeProductId = product.Id;
        subscriptionPlan.StripePriceId = basePrice.Id;
        subscriptionPlan.StripePerTruckPriceId = perTruckPrice.Id;
        await masterUow.Repository<SubscriptionPlan>().AddAsync(subscriptionPlan, ct);
        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
