using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetAccidentReportsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAccidentReportsQuery, PagedResult<AccidentReportDto>>
{
    public Task<PagedResult<AccidentReportDto>> Handle(GetAccidentReportsQuery req, CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<AccidentReport>().Query();

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

        if (req.Severity.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.Severity == req.Severity);
        }

        if (req.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.AccidentDateTime >= req.FromDate);
        }

        if (req.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.AccidentDateTime <= req.ToDate);
        }

        baseQuery = baseQuery.OrderBy(r => r.AccidentDateTime, descending: true);

        var totalItems = baseQuery.Count();
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var dtos = baseQuery.Select(r => r.ToDto()).ToArray();

        return Task.FromResult(PagedResult<AccidentReportDto>.Succeed(dtos, totalItems, req.PageSize));
    }
}
