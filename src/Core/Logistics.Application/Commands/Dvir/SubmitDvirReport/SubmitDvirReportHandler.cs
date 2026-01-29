using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class SubmitDvirReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<SubmitDvirReportCommand, Result<DvirReportDto>>
{
    public async Task<Result<DvirReportDto>> Handle(SubmitDvirReportCommand req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<DvirReport>().GetByIdAsync(req.ReportId, ct);
        if (report is null)
        {
            return Result<DvirReportDto>.Fail("DVIR report not found.");
        }

        if (report.Status != DvirStatus.Draft)
        {
            return Result<DvirReportDto>.Fail("Only draft reports can be submitted.");
        }

        if (!string.IsNullOrEmpty(req.DriverSignature))
        {
            report.DriverSignature = req.DriverSignature;
        }

        if (string.IsNullOrEmpty(report.DriverSignature))
        {
            return Result<DvirReportDto>.Fail("Driver signature is required to submit the report.");
        }

        report.Status = report.HasDefects ? DvirStatus.Submitted : DvirStatus.Cleared;
        tenantUow.Repository<DvirReport>().Update(report);
        await tenantUow.SaveChangesAsync(ct);

        return Result<DvirReportDto>.Ok(report.ToDto());
    }
}
