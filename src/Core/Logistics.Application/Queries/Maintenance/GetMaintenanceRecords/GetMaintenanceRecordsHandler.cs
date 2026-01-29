using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetMaintenanceRecordsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetMaintenanceRecordsQuery, PagedResult<MaintenanceRecordDto>>
{
    public Task<PagedResult<MaintenanceRecordDto>> Handle(GetMaintenanceRecordsQuery req, CancellationToken ct)
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

        baseQuery = baseQuery.OrderBy(r => r.ServiceDate, descending: true);

        var totalItems = baseQuery.Count();
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var dtos = baseQuery.Select(r => r.ToDto()).ToArray();

        return Task.FromResult(PagedResult<MaintenanceRecordDto>.Succeed(dtos, totalItems, req.PageSize));
    }
}
