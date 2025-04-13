using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class AddressMapper
{
    public static AddressDto ToDto(this Address entity)
    {
        return new AddressDto
        {
            Line1 = entity.Line1,
            Line2 = entity.Line2,
            City = entity.City,
            State = entity.State,
            ZipCode = entity.ZipCode,
            Country = entity.Country
        };
    }

    public static Address ToEntity(this AddressDto dto)
    {
        return new Address
        {
            Line1 = dto.Line1,
            Line2 = dto.Line2,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country
        };
    }
}
