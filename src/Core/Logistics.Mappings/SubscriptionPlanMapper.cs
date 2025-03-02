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
            Price = entity.Price,
            StripePriceId = entity.StripePriceId,
            HasTrial = entity.HasTrial,
        };
    }
}
