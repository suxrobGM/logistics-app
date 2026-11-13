using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.CustomerUsers.Queries;

internal sealed class GetCustomerUserByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetCustomerUserByIdQuery, CustomerUser, CustomerUserDto>(tenantUow)
{
    protected override CustomerUserDto MapToDto(CustomerUser entity) => entity.ToDto();
}
