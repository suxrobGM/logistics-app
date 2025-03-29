using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PermissionMapper
{
    public static PermissionDto ToDto(this AppRoleClaim entity)
    {
        return new PermissionDto()
        {
            Name = entity.ClaimValue
        };
    }
    
    public static PermissionDto ToDto(this TenantRoleClaim entity)
    {
        return new PermissionDto()
        {
            Name = entity.ClaimValue
        };
    }
}
