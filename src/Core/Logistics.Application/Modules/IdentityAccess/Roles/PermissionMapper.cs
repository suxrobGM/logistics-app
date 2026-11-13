using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Roles;

[Mapper]
public static partial class PermissionMapper
{
    [MapperIgnoreSource(nameof(AppRoleClaim.Role))]
    [MapProperty(nameof(AppRoleClaim.ClaimValue), nameof(PermissionDto.Name))]
    public static partial PermissionDto ToDto(this AppRoleClaim entity);

    [MapperIgnoreSource(nameof(TenantRoleClaim.Role))]
    [MapProperty(nameof(TenantRoleClaim.ClaimValue), nameof(PermissionDto.Name))]
    public static partial PermissionDto ToDto(this TenantRoleClaim entity);
}
