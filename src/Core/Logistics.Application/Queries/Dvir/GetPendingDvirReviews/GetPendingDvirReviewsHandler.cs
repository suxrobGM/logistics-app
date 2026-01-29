using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPendingDvirReviewsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetPendingDvirReviewsQuery, Result<List<DvirReportDto>>>
{
    public async Task<Result<List<DvirReportDto>>> Handle(GetPendingDvirReviewsQuery req, CancellationToken ct)
    {
        var reports = await tenantUow.Repository<DvirReport>().GetListAsync(
            r => r.Status == DvirStatus.Submitted || r.Status == DvirStatus.RequiresRepair,
            ct);

        var dtos = reports
            .OrderBy(r => r.InspectionDate)
            .Select(r => r.ToDto())
            .ToList();

        return Result<List<DvirReportDto>>.Ok(dtos);
    }
}
