using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Accidents.Queries;

internal sealed class GetAccidentReportsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetAccidentReportsQuery, PagedResult<AccidentReportDto>>
{
    public async Task<PagedResult<AccidentReportDto>> Handle(GetAccidentReportsQuery req, CancellationToken ct)
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

        return await baseQuery.ToPagedResultAsync(req, r => r.ToDto(), ct, defaultOrderBy: "-AccidentDateTime");
    }
}
