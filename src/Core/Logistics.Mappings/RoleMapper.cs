using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class RoleMapper
{
    [MapperIgnoreSource(nameof(AppRole.Claims))]
    public static partial RoleDto ToDto(this AppRole entity);

    [MapperIgnoreSource(nameof(TenantRole.Employees))]
    [MapperIgnoreSource(nameof(TenantRole.Claims))]
    public static partial RoleDto ToDto(this TenantRole entity);
}
