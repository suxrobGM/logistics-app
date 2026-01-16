using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class UserTenantAccessMapper
{
    /// <summary>
    /// Maps a UserTenantAccess entity to UserTenantAccessDto.
    /// </summary>
    [UserMapping(Default = true)]
    public static UserTenantAccessDto ToDto(this UserTenantAccess entity)
    {
        return new UserTenantAccessDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            TenantName = entity.Tenant?.Name ?? string.Empty,
            CompanyName = entity.Tenant?.CompanyName,
            CustomerName = entity.CustomerName,
            IsActive = entity.IsActive,
            LastAccessedAt = entity.LastAccessedAt
        };
    }
}
