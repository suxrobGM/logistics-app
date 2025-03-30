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
            Tenant = GetTenantDto(entity.Tenant),
            Plan = entity.Plan.ToDto(),
            StripeSubscriptionId = entity.StripeSubscriptionId,
            StripeCustomerId = entity.StripeCustomerId,
        };
    }

    private static TenantDto GetTenantDto(Tenant entity)
    {
        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CompanyName = entity.CompanyName,
            BillingEmail = entity.BillingEmail,
            DotNumber = entity.DotNumber,
            CompanyAddress = entity.CompanyAddress.ToDto(),
        };
    }
}
