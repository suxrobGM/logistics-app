using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Maintenance.Queries;

internal sealed class GetMaintenanceRecordByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetMaintenanceRecordByIdQuery, MaintenanceRecord, MaintenanceRecordDto>(tenantUow)
{
    protected override MaintenanceRecordDto MapToDto(MaintenanceRecord entity) => entity.ToDto();
}
