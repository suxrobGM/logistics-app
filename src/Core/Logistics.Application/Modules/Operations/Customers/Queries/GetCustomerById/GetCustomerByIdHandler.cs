using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Customers.Queries;

internal sealed class GetCustomerByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetCustomerByIdQuery, Customer, CustomerDto>(tenantUow)
{
    protected override CustomerDto MapToDto(Customer entity) => entity.ToDto();
}
