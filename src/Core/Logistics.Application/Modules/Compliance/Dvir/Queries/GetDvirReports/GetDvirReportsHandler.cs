using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Modules.Compliance.Dvir.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

internal sealed class GetDvirReportsHandler(ITenantUnitOfWork tenantUow, IDvirReportListMapper listMapper)
    : IRequestHandler<GetDvirReportsQuery, PagedResult<DvirReportDto>>
{
    private static readonly SortFields<DvirReport> Sorts = new SortFields<DvirReport>("-inspectionDate")
        .Add("inspectionDate", x => x.InspectionDate)
        .Add("truckNumber", x => x.Truck.Number)
        .Add("driverName", x => x.Driver.FirstName);

    public async Task<PagedResult<DvirReportDto>> Handle(GetDvirReportsQuery req, CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<DvirReport>().Query();

        if (req.TruckId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.TruckId == req.TruckId);
        }

        if (req.DriverId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.DriverId == req.DriverId);
        }

        if (req.Status.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.Status == req.Status);
        }

        if (req.Type.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.Type == req.Type);
        }

        if (req.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.InspectionDate >= req.FromDate);
        }

        if (req.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.InspectionDate <= req.ToDate);
        }

        return await baseQuery.ToPagedResultAsync(req, Sorts, listMapper.MapAsync, ct);
    }
}
