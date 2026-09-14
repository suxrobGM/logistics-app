using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

internal sealed class GetTruckOpenDvirDefectsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTruckOpenDvirDefectsQuery, Result<List<DvirDefectDto>>>
{
    public async Task<Result<List<DvirDefectDto>>> Handle(GetTruckOpenDvirDefectsQuery req, CancellationToken ct)
    {
        var reports = tenantUow.Repository<DvirReport>().Query()
            .Where(r => r.TruckId == req.TruckId && r.Status != DvirStatus.Draft);

        var defects = new List<DvirDefectDto>();

        // A newer inspection of the same type re-checks the same items, so it supersedes older ones.
        foreach (var type in Enum.GetValues<DvirType>())
        {
            var latest = await reports
                .Where(r => r.Type == type)
                .OrderByDescending(r => r.InspectionDate)
                .FirstOrDefaultAsync(ct);

            if (latest is not null)
            {
                defects.AddRange(latest.Defects.Where(d => !d.IsCorrected).Select(d => d.ToDto()));
            }
        }

        return Result<List<DvirDefectDto>>.Ok(defects);
    }
}
