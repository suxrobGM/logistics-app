using Logistics.Application.Modules.IdentityAccess.Subscriptions;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Tenants;

[Mapper]
public static partial class TenantMapper
{
    [MapperIgnoreSource(nameof(Tenant.Subscription))]
    [MapperIgnoreSource(nameof(Tenant.Users))]
    [MapperIgnoreSource(nameof(Tenant.FeatureConfigs))]
    [MapperIgnoreTarget(nameof(TenantDto.Subscription))]
    [MapperIgnoreTarget(nameof(TenantDto.TruckCount))]
    [MapProperty(nameof(Tenant.LogoPath), nameof(TenantDto.LogoUrl))]
    private static partial TenantDto MapTenant(Tenant entity);

    extension(Tenant entity)
    {
        public TenantDto ToDto(int? truckCount = null)
        {
            var dto = MapTenant(entity);
            return dto with { Subscription = entity.Subscription?.ToDto(), TruckCount = truckCount };
        }
    }
}
