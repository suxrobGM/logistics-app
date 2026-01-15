using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class CustomerUserMapper
{
    /// <summary>
    /// Maps a CustomerUser entity to CustomerUserDto.
    /// </summary>
    [UserMapping(Default = true)]
    public static CustomerUserDto ToDto(this CustomerUser entity)
    {
        return new CustomerUserDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            CustomerId = entity.CustomerId,
            CustomerName = entity.Customer?.Name ?? string.Empty,
            Email = entity.Email,
            DisplayName = entity.DisplayName,
            IsActive = entity.IsActive,
            LastLoginAt = entity.LastLoginAt,
            CreatedAt = entity.CreatedAt
        };
    }
}
