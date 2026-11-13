using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Logistics.Application.Modules.Compliance.Dvir.Services;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

internal sealed class GetPendingDvirReviewsHandler(ITenantUnitOfWork tenantUow, IDvirReportListMapper listMapper)
    : IRequestHandler<GetPendingDvirReviewsQuery, Result<List<DvirReportDto>>>
{
    public async Task<Result<List<DvirReportDto>>> Handle(GetPendingDvirReviewsQuery req, CancellationToken ct)
    {
        var reports = await tenantUow.Repository<DvirReport>().Query()
            .Where(r => r.Status == DvirStatus.Submitted || r.Status == DvirStatus.RequiresRepair)
            .OrderBy(r => r.InspectionDate)
            .ToListAsync(ct);

        var dtos = (await listMapper.MapAsync(reports, ct)).ToList();

        return Result<List<DvirReportDto>>.Ok(dtos);
    }
}
