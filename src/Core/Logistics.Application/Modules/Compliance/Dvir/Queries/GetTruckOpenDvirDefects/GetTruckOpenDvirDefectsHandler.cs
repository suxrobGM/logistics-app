using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

internal sealed class GetTruckOpenDvirDefectsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTruckOpenDvirDefectsQuery, Result<List<DvirDefectDto>>>
{
    public async Task<Result<List<DvirDefectDto>>> Handle(GetTruckOpenDvirDefectsQuery req, CancellationToken ct)
    {
        var reports = tenantUow.Repository<DvirReport>().Query()
            .Where(r => r.TruckId == req.TruckId && r.Status != DvirStatus.Draft);

        // A newer inspection of the same type re-checks the same items, so it supersedes older ones.
        var latestReportIds = await reports
            .GroupBy(r => r.Type)
            .Select(g => g.OrderByDescending(r => r.InspectionDate).Select(r => r.Id).First())
            .ToListAsync(ct);

        var defects = await reports
            .Where(r => latestReportIds.Contains(r.Id))
            .OrderBy(r => r.Type)
            .SelectMany(r => r.Defects)
            .Where(d => !d.IsCorrected)
            .ToListAsync(ct);

        return Result<List<DvirDefectDto>>.Ok(defects.Select(d => d.ToDto()).ToList());
    }
}
