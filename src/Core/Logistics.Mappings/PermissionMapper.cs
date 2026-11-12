using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PermissionMapper
{
    extension(AppRoleClaim entity)
    {
        public PermissionDto ToDto()
        {
            return new PermissionDto()
            {
                Name = entity.ClaimValue
            };
        }
    }

    extension(TenantRoleClaim entity)
    {
        public PermissionDto ToDto()
        {
            return new PermissionDto()
            {
                Name = entity.ClaimValue
            };
        }
    }
}
