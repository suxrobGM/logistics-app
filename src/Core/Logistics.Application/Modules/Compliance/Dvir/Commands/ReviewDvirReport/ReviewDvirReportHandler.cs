using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Commands;

internal sealed class ReviewDvirReportHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IRequestHandler<ReviewDvirReportCommand, Result<DvirReportDto>>
{
    public async Task<Result<DvirReportDto>> Handle(ReviewDvirReportCommand req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<DvirReport>().GetByIdAsync(req.ReportId, ct);
        if (report is null)
        {
            return Result<DvirReportDto>.Fail("DVIR report not found.", ErrorCodes.NotFound);
        }

        if (report.Status != DvirStatus.Submitted && report.Status != DvirStatus.RequiresRepair)
        {
            return Result<DvirReportDto>.Fail("Only submitted reports or reports requiring repair can be reviewed.");
        }

        var reviewerId = currentUserService.GetUserId();
        if (reviewerId is null)
        {
            return Result<DvirReportDto>.Fail("Unable to determine the current user.");
        }

        var reviewer = await tenantUow.Repository<Employee>().GetByIdAsync(reviewerId.Value, ct);
        if (reviewer is null)
        {
            return Result<DvirReportDto>.Fail("Reviewer not found.", ErrorCodes.NotFound);
        }

        report.ReviewedById = reviewer.Id;
        report.ReviewedBy = reviewer;
        report.ReviewedAt = DateTime.UtcNow;
        report.DefectsCorrected = req.DefectsCorrected;
        report.MechanicSignature = req.MechanicSignature;
        report.MechanicNotes = req.MechanicNotes;

        if (req.DefectsCorrected)
        {
            report.Status = DvirStatus.Cleared;
            foreach (var defect in report.Defects.Where(d => !d.IsCorrected))
            {
                defect.IsCorrected = true;
                defect.CorrectedAt = DateTime.UtcNow;
                defect.CorrectedById = reviewer.Id;
            }
        }
        else
        {
            report.Status = DvirStatus.RequiresRepair;
        }

        await tenantUow.SaveChangesAsync(ct);

        return Result<DvirReportDto>.Ok(report.ToDto());
    }
}
