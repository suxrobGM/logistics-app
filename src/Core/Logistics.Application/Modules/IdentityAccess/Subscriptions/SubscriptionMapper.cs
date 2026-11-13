using Logistics.Application.Modules.IdentityAccess.Tenants;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions;

[Mapper]
public static partial class SubscriptionMapper
{
    [MapperIgnoreSource(nameof(Subscription.Tenant))]
    [MapperIgnoreSource(nameof(Subscription.Plan))]
    [MapperIgnoreTarget(nameof(SubscriptionDto.Tenant))]
    [MapperIgnoreTarget(nameof(SubscriptionDto.Plan))]
    private static partial SubscriptionDto MapSubscription(Subscription entity);

    extension(Subscription entity)
    {
        public SubscriptionDto ToDto()
        {
            var dto = MapSubscription(entity);
            return dto with { Tenant = GetTenantDto(entity.Tenant), Plan = entity.Plan.ToDto() };
        }
    }

    /// <summary>
    /// A trimmed tenant: <see cref="TenantMapper"/> would map the tenant's subscription back into it.
    /// </summary>
    private static TenantDto GetTenantDto(Tenant entity)
    {
        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CompanyName = entity.CompanyName,
            BillingEmail = entity.BillingEmail,
            DotNumber = entity.DotNumber,
            CompanyAddress = entity.CompanyAddress,
        };
    }
}
