using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class RoleMapper
{
    extension(AppRole entity)
    {
        public RoleDto ToDto()
        {
            return new RoleDto()
            {
                Name = entity.Name,
                DisplayName = entity.DisplayName
            };
        }
    }

    extension(TenantRole entity)
    {
        public RoleDto ToDto()
        {
            return new RoleDto()
            {
                Name = entity.Name,
                DisplayName = entity.DisplayName
            };
        }
    }
}
