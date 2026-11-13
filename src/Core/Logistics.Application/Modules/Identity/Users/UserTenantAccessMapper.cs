using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Identity.Users;

[Mapper]
public static partial class UserTenantAccessMapper
{
    [MapperIgnoreSource(nameof(UserTenantAccess.User))]
    [MapperIgnoreSource(nameof(UserTenantAccess.Tenant))]
    [MapperIgnoreTarget(nameof(UserTenantAccessDto.TenantName))]
    [MapperIgnoreTarget(nameof(UserTenantAccessDto.CompanyName))]
    [MapperIgnoreTarget(nameof(UserTenantAccessDto.Settings))]
    private static partial UserTenantAccessDto MapAccess(UserTenantAccess entity);

    extension(UserTenantAccess entity)
    {
        public UserTenantAccessDto ToDto()
        {
            var dto = MapAccess(entity);
            dto.TenantName = entity.Tenant?.Name ?? string.Empty;
            dto.CompanyName = entity.Tenant?.CompanyName;
            dto.Settings = entity.Tenant?.Settings;
            return dto;
        }
    }
}
