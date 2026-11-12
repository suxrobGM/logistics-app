using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Logistics.Application.Modules.Compliance.Dvir.Services;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

internal sealed class GetPendingDvirReviewsHandler(ITenantUnitOfWork tenantUow, IDvirReportListMapper listMapper)
    : IRequestHandler<GetPendingDvirReviewsQuery, Result<List<DvirReportDto>>>
{
    public async Task<Result<List<DvirReportDto>>> Handle(GetPendingDvirReviewsQuery req, CancellationToken ct)
    {
        var reports = await tenantUow.Repository<DvirReport>().GetListAsync(
            r => r.Status == DvirStatus.Submitted || r.Status == DvirStatus.RequiresRepair,
            ct);

        var ordered = reports.OrderBy(r => r.InspectionDate).ToList();
        var dtos = (await listMapper.MapAsync(ordered, ct)).ToList();

        return Result<List<DvirReportDto>>.Ok(dtos);
    }
}
