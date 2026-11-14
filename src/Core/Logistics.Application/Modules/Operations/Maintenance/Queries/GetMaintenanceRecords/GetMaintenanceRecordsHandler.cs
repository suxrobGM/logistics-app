using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Common.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Maintenance.Queries;

internal sealed class GetMaintenanceRecordsHandler(ITenantUnitOfWork tenantUow, IDisplayNameLookup names)
    : IRequestHandler<GetMaintenanceRecordsQuery, PagedResult<MaintenanceRecordDto>>
{
    private static readonly SortFields<MaintenanceRecord> Sorts = new SortFields<MaintenanceRecord>("-serviceDate")
        .Add("serviceDate", x => x.ServiceDate)
        .Add("truckNumber", x => x.Truck.Number)
        .Add("odometerMeters", x => x.OdometerMeters)
        .Add("totalCost", x => x.TotalCost);

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
            baseQuery = baseQuery.Where(r => r.ServiceDate >= DateOnly.FromDateTime(req.FromDate.Value));
        }

        if (req.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.ServiceDate <= DateOnly.FromDateTime(req.ToDate.Value));
        }

        return await baseQuery.ToPagedResultAsync(req, Sorts, MapPageAsync, ct);
    }

    private async Task<IEnumerable<MaintenanceRecordDto>> MapPageAsync(
        IReadOnlyList<MaintenanceRecord> records, CancellationToken ct)
    {
        var recordIds = records.Select(r => r.Id).ToArray();
        var parts = (await tenantUow.Repository<MaintenancePart>().Query()
            .Where(p => recordIds.Contains(p.MaintenanceRecordId))
            .ToListAsync(ct)).ToLookup(p => p.MaintenanceRecordId);

        var employeeNames = await names.GetEmployeeNamesAsync(records.Select(r => r.PerformedById), ct);
        var truckNumbers = await names.GetTruckNumbersAsync(records.Select(r => (Guid?)r.TruckId), ct);

        return records.Select(r => r.ToDto(
            truckNumbers.Find(r.TruckId),
            employeeNames.Find(r.PerformedById),
            parts[r.Id].Select(p => p.ToDto())));
    }
}
