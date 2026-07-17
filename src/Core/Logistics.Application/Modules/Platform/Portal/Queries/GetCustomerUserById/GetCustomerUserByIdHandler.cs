using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

internal sealed class GetCustomerUserByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetCustomerUserByIdQuery, CustomerUser, CustomerUserDto>(tenantUow)
{
    protected override CustomerUserDto MapToDto(CustomerUser entity) => entity.ToDto();
}
