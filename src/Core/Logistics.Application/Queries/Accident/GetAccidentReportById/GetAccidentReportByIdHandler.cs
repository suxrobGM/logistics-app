using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetAccidentReportByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAccidentReportByIdQuery, Result<AccidentReportDto>>
{
    public async Task<Result<AccidentReportDto>> Handle(GetAccidentReportByIdQuery req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<AccidentReport>().GetByIdAsync(req.Id, ct);

        if (report is null)
        {
            return Result<AccidentReportDto>.Fail("Accident report not found.");
        }

        return Result<AccidentReportDto>.Ok(report.ToDto());
    }
}
