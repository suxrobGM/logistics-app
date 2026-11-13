using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Commands;

internal sealed class DismissDvirReportHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IRequestHandler<DismissDvirReportCommand, Result<DvirReportDto>>
{
    public async Task<Result<DvirReportDto>> Handle(DismissDvirReportCommand req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<DvirReport>().GetByIdAsync(req.ReportId, ct);
        if (report is null)
        {
            return Result<DvirReportDto>.Fail("DVIR report not found.", ErrorCodes.NotFound);
        }

        if (report.Status != DvirStatus.Submitted)
        {
            return Result<DvirReportDto>.Fail("Only submitted reports can be dismissed.");
        }

        var reviewerId = currentUserService.GetUserId();
        if (reviewerId is null)
        {
            return Result<DvirReportDto>.Fail("Unable to determine the current user.");
        }

        report.Status = DvirStatus.Cleared;
        report.ReviewedById = reviewerId.Value;
        report.ReviewedAt = DateTime.UtcNow;
        report.MechanicNotes = string.IsNullOrEmpty(req.Notes)
            ? "Dismissed - No action required."
            : req.Notes;
        report.DefectsCorrected = !report.HasDefects || report.Defects.Count == 0;

        await tenantUow.SaveChangesAsync(ct);

        return Result<DvirReportDto>.Ok(report.ToDto());
    }
}
