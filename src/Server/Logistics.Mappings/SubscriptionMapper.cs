using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class SubscriptionMapper
{
    public static SubscriptionDto ToDto(this Subscription entity)
    {
        return new SubscriptionDto
        {
            Id = entity.Id,
            Status = entity.Status,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            NextPaymentDate = entity.NextPaymentDate,
            TrialEndDate = entity.TrialEndDate,
            Tenant = entity.Tenant.ToDto(),
            Plan = entity.Plan.ToDto(),
        };
    }
}
