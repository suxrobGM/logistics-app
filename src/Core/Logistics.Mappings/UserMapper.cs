using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class UserMapper
{
    public static UserDto ToDto(this User entity)
    {
        var dto = new UserDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Role = entity.AppRole?.Name,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            TenantId = entity.TenantId,
            TenantName = entity.Tenant?.Name
        };
        return dto;
    }
}
