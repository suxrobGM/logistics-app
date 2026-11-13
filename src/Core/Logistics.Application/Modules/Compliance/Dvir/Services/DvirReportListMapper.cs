using Logistics.Application.Common.Services;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Dvir.Services;

internal sealed class DvirReportListMapper(ITenantUnitOfWork tenantUow, IDisplayNameLookup names)
    : IDvirReportListMapper
{
    public async Task<IEnumerable<DvirReportDto>> MapAsync(IReadOnlyList<DvirReport> reports, CancellationToken ct)
    {
        var reportIds = reports.Select(r => r.Id).ToArray();
        var defects = await tenantUow.Repository<DvirDefect>().Query()
            .Where(d => reportIds.Contains(d.DvirReportId))
            .ToListAsync(ct);

        var employeeIds = reports.SelectMany(r => new Guid?[] { r.DriverId, r.ReviewedById })
            .Concat(defects.Select(d => d.CorrectedById));
        var employeeNames = await names.GetEmployeeNamesAsync(employeeIds, ct);
        var truckNumbers = await names.GetTruckNumbersAsync(reports.Select(r => (Guid?)r.TruckId), ct);
        var defectsByReport = defects.ToLookup(d => d.DvirReportId);

        return reports.Select(r => r.ToDto(
            truckNumbers.Find(r.TruckId),
            employeeNames.Find(r.DriverId),
            employeeNames.Find(r.ReviewedById),
            defectsByReport[r.Id].Select(d => d.ToDto(employeeNames.Find(d.CorrectedById)))));
    }
}
