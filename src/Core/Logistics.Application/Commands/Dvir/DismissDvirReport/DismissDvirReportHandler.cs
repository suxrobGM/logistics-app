using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DismissDvirReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DismissDvirReportCommand, Result<DvirReportDto>>
{
    public async Task<Result<DvirReportDto>> Handle(DismissDvirReportCommand req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<DvirReport>().GetByIdAsync(req.ReportId, ct);
        if (report is null)
        {
            return Result<DvirReportDto>.Fail("DVIR report not found.");
        }

        if (report.Status != DvirStatus.Submitted)
        {
            return Result<DvirReportDto>.Fail("Only submitted reports can be dismissed.");
        }

        // Dismiss directly clears the report (typically for reports with no defects)
        report.Status = DvirStatus.Cleared;
        report.ReviewedById = req.DismissedById;
        report.ReviewedAt = DateTime.UtcNow;
        report.MechanicNotes = string.IsNullOrEmpty(req.Notes)
            ? "Dismissed - No action required."
            : req.Notes;
        report.DefectsCorrected = !report.HasDefects || report.Defects.Count == 0;

        tenantUow.Repository<DvirReport>().Update(report);
        await tenantUow.SaveChangesAsync(ct);

        return Result<DvirReportDto>.Ok(report.ToDto());
    }
}
