using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Queries;

internal sealed class GetDriverBehaviorEventByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetDriverBehaviorEventByIdQuery, DriverBehaviorEvent, DriverBehaviorEventDto>(tenantUow)
{
    protected override DriverBehaviorEventDto MapToDto(DriverBehaviorEvent entity) => entity.ToDto();
}
