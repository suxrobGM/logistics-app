using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Maintenance.Queries;

internal sealed class GetMaintenanceRecordsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetMaintenanceRecordsQuery, PagedResult<MaintenanceRecordDto>>
{
    public async Task<PagedResult<MaintenanceRecordDto>> Handle(GetMaintenanceRecordsQuery req, CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<MaintenanceRecord>().Query();

        if (req.TruckId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.TruckId == req.TruckId);
        }

        if (req.Type.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.MaintenanceType == req.Type);
        }

        if (req.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.ServiceDate >= req.FromDate);
        }

        if (req.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.ServiceDate <= req.ToDate);
        }

        return await baseQuery.ToPagedResultAsync(req, r => r.ToDto(), ct, defaultOrderBy: "-ServiceDate");
    }
}
