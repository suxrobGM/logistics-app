using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Identity.Users;

[Mapper]
public static partial class UserMapper
{
    [MapperIgnoreSource(nameof(User.Tenant))]
    [MapperIgnoreTarget(nameof(UserDto.TenantName))]
    [MapperIgnoreTarget(nameof(UserDto.Role))]
    private static partial UserDto MapUser(User entity);

    extension(User entity)
    {
        public UserDto ToDto(string? role = null)
        {
            var dto = MapUser(entity);
            return dto with { Role = role, TenantName = entity.Tenant?.Name };
        }
    }
}
