using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetMaintenanceRecordByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetMaintenanceRecordByIdQuery, Result<MaintenanceRecordDto>>
{
    public async Task<Result<MaintenanceRecordDto>> Handle(GetMaintenanceRecordByIdQuery req, CancellationToken ct)
    {
        var record = await tenantUow.Repository<MaintenanceRecord>().GetByIdAsync(req.Id, ct);

        if (record is null)
        {
            return Result<MaintenanceRecordDto>.Fail($"Could not find maintenance record with ID '{req.Id}'");
        }

        return Result<MaintenanceRecordDto>.Ok(record.ToDto());
    }
}
