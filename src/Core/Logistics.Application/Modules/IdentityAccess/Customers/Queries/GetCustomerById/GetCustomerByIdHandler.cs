using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Queries;

internal sealed class GetCustomerByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetCustomerByIdQuery, Customer, CustomerDto>(tenantUow)
{
    protected override CustomerDto MapToDto(Customer entity) => entity.ToDto();
}
