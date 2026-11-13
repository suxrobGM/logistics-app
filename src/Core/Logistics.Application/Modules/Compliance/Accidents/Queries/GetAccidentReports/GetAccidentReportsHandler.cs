using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Logistics.Application.Modules.Common.Services;

namespace Logistics.Application.Modules.Compliance.Accidents.Queries;

internal sealed class GetAccidentReportsHandler(ITenantUnitOfWork tenantUow, IDisplayNameLookup names)
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

        return await baseQuery.ToPagedResultAsync(req, MapPageAsync, ct, defaultOrderBy: "-AccidentDateTime");
    }

    private async Task<IEnumerable<AccidentReportDto>> MapPageAsync(
        IReadOnlyList<AccidentReport> reports, CancellationToken ct)
    {
        var reportIds = reports.Select(r => r.Id).ToArray();
        var witnesses = (await tenantUow.Repository<AccidentWitness>().Query()
            .Where(w => reportIds.Contains(w.AccidentReportId))
            .ToListAsync(ct)).ToLookup(w => w.AccidentReportId);
        var thirdParties = (await tenantUow.Repository<AccidentThirdParty>().Query()
            .Where(tp => reportIds.Contains(tp.AccidentReportId))
            .ToListAsync(ct)).ToLookup(tp => tp.AccidentReportId);

        var employeeNames = await names.GetEmployeeNamesAsync(
            reports.SelectMany(r => new Guid?[] { r.DriverId, r.ReviewedById }), ct);
        var truckNumbers = await names.GetTruckNumbersAsync(reports.Select(r => (Guid?)r.TruckId), ct);

        return reports.Select(r => r.ToDto(
            truckNumbers.Find(r.TruckId),
            employeeNames.Find(r.DriverId),
            employeeNames.Find(r.ReviewedById),
            witnesses[r.Id].Select(w => w.ToDto()),
            thirdParties[r.Id].Select(tp => tp.ToDto())));
    }
}
