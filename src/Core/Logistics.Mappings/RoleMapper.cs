using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class RoleMapper
{
    public static RoleDto ToDto(this AppRole entity)
    {
        return new RoleDto()
        {
            Name = entity.Name,
            DisplayName = entity.DisplayName
        };
    }
    
    public static RoleDto ToDto(this TenantRole entity)
    {
        return new RoleDto()
        {
            Name = entity.Name,
            DisplayName = entity.DisplayName
        };
    }
}
