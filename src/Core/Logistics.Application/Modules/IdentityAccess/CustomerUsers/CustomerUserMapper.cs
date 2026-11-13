using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.CustomerUsers;

[Mapper]
public static partial class CustomerUserMapper
{
    [MapperIgnoreSource(nameof(CustomerUser.Customer))]
    [MapperIgnoreTarget(nameof(CustomerUserDto.CustomerName))]
    private static partial CustomerUserDto MapCustomerUser(CustomerUser entity);

    extension(CustomerUser entity)
    {
        public CustomerUserDto ToDto()
        {
            var dto = MapCustomerUser(entity);
            dto.CustomerName = entity.Customer?.Name ?? string.Empty;
            return dto;
        }
    }
}
