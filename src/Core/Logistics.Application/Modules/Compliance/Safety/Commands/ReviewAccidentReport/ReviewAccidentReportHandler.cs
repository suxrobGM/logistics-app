using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Commands;

internal sealed class ReviewAccidentReportHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IRequestHandler<ReviewAccidentReportCommand, Result<AccidentReportDto>>
{
    public async Task<Result<AccidentReportDto>> Handle(ReviewAccidentReportCommand req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<AccidentReport>().GetByIdAsync(req.ReportId, ct);
        if (report is null)
        {
            return Result<AccidentReportDto>.Fail("Accident report not found.", ErrorCodes.NotFound);
        }

        if (report.Status != AccidentReportStatus.Submitted)
        {
            return Result<AccidentReportDto>.Fail("Only submitted reports can be reviewed.");
        }

        var reviewerId = currentUserService.GetUserId();
        if (reviewerId is null)
        {
            return Result<AccidentReportDto>.Fail("Unable to determine the current user.");
        }

        report.Status = AccidentReportStatus.UnderReview;
        report.ReviewedById = reviewerId.Value;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewNotes = req.ReviewNotes;

        await tenantUow.SaveChangesAsync(ct);

        return Result<AccidentReportDto>.Ok(report.ToDto());
    }
}
