using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDvirReportsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetDvirReportsQuery, PagedResult<DvirReportDto>>
{
    public Task<PagedResult<DvirReportDto>> Handle(GetDvirReportsQuery req, CancellationToken ct)
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

        baseQuery = baseQuery.OrderBy(r => r.InspectionDate, descending: true);

        var totalItems = baseQuery.Count();
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var dtos = baseQuery.Select(r => r.ToDto()).ToArray();

        return Task.FromResult(PagedResult<DvirReportDto>.Succeed(dtos, totalItems, req.PageSize));
    }
}
