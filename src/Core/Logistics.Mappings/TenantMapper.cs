using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TenantMapper
{
    extension(Tenant entity)
    {
        public TenantDto ToDto(int? truckCount = null)
        {
            return new TenantDto
            {
                Id = entity.Id,
                Name = entity.Name,
                CompanyName = entity.CompanyName,
                BillingEmail = entity.BillingEmail,
                DotNumber = entity.DotNumber,
                McNumber = entity.McNumber,
                VatNumber = entity.VatNumber,
                EoriNumber = entity.EoriNumber,
                CompanyRegistrationNumber = entity.CompanyRegistrationNumber,
                TaxResidencyCountry = entity.TaxResidencyCountry,
                CompanyAddress = entity.CompanyAddress,
                StripeCustomerId = entity.StripeCustomerId,
                LogoUrl = entity.LogoPath,
                PhoneNumber = entity.PhoneNumber,
                Subscription = entity.Subscription?.ToDto(),
                TruckCount = truckCount,
                IsSubscriptionRequired = entity.IsSubscriptionRequired,
                Settings = entity.Settings,
                Presets = entity.Presets
            };
        }
    }
}
