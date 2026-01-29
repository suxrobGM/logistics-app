using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDvirReportByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetDvirReportByIdQuery, Result<DvirReportDto>>
{
    public async Task<Result<DvirReportDto>> Handle(GetDvirReportByIdQuery req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<DvirReport>().GetByIdAsync(req.Id, ct);

        if (report is null)
        {
            return Result<DvirReportDto>.Fail("DVIR report not found.");
        }

        return Result<DvirReportDto>.Ok(report.ToDto());
    }
}
