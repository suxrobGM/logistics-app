using Logistics.Domain.Entities;
using Logistics.Shared.Models;

using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class CustomerMapper
{
    [MapperIgnoreSource(nameof(Customer.Invoices))]
    [MapperIgnoreSource(nameof(Customer.DomainEvents))]
    public static partial CustomerDto ToDto(this Customer entity);
}
