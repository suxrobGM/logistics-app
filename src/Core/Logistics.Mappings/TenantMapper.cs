using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TenantMapper
{
    public static TenantDto ToDto(this Tenant entity, bool includeConnectionString = false, int? truckCount = null)
    {
        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CompanyName = entity.CompanyName,
            BillingEmail = entity.BillingEmail,
            DotNumber = entity.DotNumber,
            CompanyAddress = entity.CompanyAddress,
            ConnectionString = includeConnectionString ? entity.ConnectionString : null,
            StripeCustomerId = entity.StripeCustomerId,
            LogoUrl = entity.LogoPath,
            PhoneNumber = entity.PhoneNumber,
            Subscription = entity.Subscription?.ToDto(),
            TruckCount = truckCount,
            IsSubscriptionRequired = entity.IsSubscriptionRequired,
            Settings = entity.Settings
        };
    }
}
