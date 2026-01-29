using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class SubmitAccidentReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<SubmitAccidentReportCommand, Result<AccidentReportDto>>
{
    public async Task<Result<AccidentReportDto>> Handle(SubmitAccidentReportCommand req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<AccidentReport>().GetByIdAsync(req.ReportId, ct);
        if (report is null)
        {
            return Result<AccidentReportDto>.Fail("Accident report not found.");
        }

        if (report.Status != AccidentReportStatus.Draft)
        {
            return Result<AccidentReportDto>.Fail("Only draft reports can be submitted.");
        }

        report.Status = AccidentReportStatus.Submitted;

        tenantUow.Repository<AccidentReport>().Update(report);
        await tenantUow.SaveChangesAsync(ct);

        return Result<AccidentReportDto>.Ok(report.ToDto());
    }
}
