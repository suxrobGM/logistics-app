using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Tenancy.Subscriptions;

[Mapper]
public static partial class SubscriptionPlanMapper
{
    [MapperIgnoreSource(nameof(SubscriptionPlan.Subscriptions))]
    [MapperIgnoreSource(nameof(SubscriptionPlan.Features))]
    [MapperIgnoreTarget(nameof(SubscriptionPlanDto.Features))]
    [MapProperty("Price.Amount", nameof(SubscriptionPlanDto.Price))]
    [MapProperty("PerTruckPrice.Amount", nameof(SubscriptionPlanDto.PerTruckPrice))]
    private static partial SubscriptionPlanDto MapPlan(SubscriptionPlan entity);

    extension(SubscriptionPlan entity)
    {
        public SubscriptionPlanDto ToDto()
        {
            var dto = MapPlan(entity);
            return dto with { Features = entity.Features.Select(f => f.Feature).ToList() };
        }
    }
}
