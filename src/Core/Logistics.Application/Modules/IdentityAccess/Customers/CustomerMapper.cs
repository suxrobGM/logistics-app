using Logistics.Domain.Entities;
using Logistics.Shared.Models;

using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Customers;

[Mapper]
public static partial class CustomerMapper
{
    [MapperIgnoreSource(nameof(Customer.Invoices))]
    [MapperIgnoreSource(nameof(Customer.FactoringCompany))]
    public static partial CustomerDto ToDto(this Customer entity);
}
