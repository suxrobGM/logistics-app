using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class CustomerUserMapper
{
    [MapperIgnoreSource(nameof(CustomerUser.Customer))]
    [MapperIgnoreTarget(nameof(CustomerUserDto.CustomerName))]
    private static partial CustomerUserDto MapCustomerUser(CustomerUser entity);

    extension(CustomerUser entity)
    {
        /// <summary>
        /// Maps a CustomerUser entity to CustomerUserDto.
        /// </summary>
        public CustomerUserDto ToDto()
        {
            var dto = MapCustomerUser(entity);
            dto.CustomerName = entity.Customer?.Name ?? string.Empty;
            return dto;
        }
    }
}
