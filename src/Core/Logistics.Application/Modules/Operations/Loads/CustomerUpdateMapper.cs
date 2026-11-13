using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Operations.Loads;

[Mapper]
public static partial class CustomerUpdateMapper
{
    public static partial CustomerUpdateDto ToDto(this CustomerUpdate entity);

    extension(IEnumerable<CustomerUpdate> entities)
    {
        public List<CustomerUpdateDto> ToDto() =>
            [.. entities.Select(ToDto)];
    }
}
