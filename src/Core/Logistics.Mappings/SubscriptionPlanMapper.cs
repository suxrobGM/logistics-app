using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class SubscriptionPlanMapper
{
    public static SubscriptionPlanDto ToDto(this SubscriptionPlan entity)
    {
        return new SubscriptionPlanDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Tier = entity.Tier,
            Price = entity.Price,
            PerTruckPrice = entity.PerTruckPrice,
            MaxTrucks = entity.MaxTrucks,
            AnnualDiscountPercent = entity.AnnualDiscountPercent,
            StripePriceId = entity.StripePriceId,
            StripeProductId = entity.StripeProductId,
            TrialPeriod = entity.TrialPeriod,
            Interval = entity.Interval,
            IntervalCount = entity.IntervalCount,
            Features = entity.Features.Select(f => f.Feature).ToList()
        };
    }
}
