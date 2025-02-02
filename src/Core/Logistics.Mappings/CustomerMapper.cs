using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            // Invoices = entity.Invoices.Select(i => i.ToDto())
        };
    }
}
