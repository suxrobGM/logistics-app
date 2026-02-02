using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ReviewAccidentReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<ReviewAccidentReportCommand, Result<AccidentReportDto>>
{
    public async Task<Result<AccidentReportDto>> Handle(ReviewAccidentReportCommand req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<AccidentReport>().GetByIdAsync(req.ReportId, ct);
        if (report is null)
        {
            return Result<AccidentReportDto>.Fail("Accident report not found.");
        }

        if (report.Status != AccidentReportStatus.Submitted)
        {
            return Result<AccidentReportDto>.Fail("Only submitted reports can be reviewed.");
        }

        report.Status = AccidentReportStatus.UnderReview;
        report.ReviewedById = req.ReviewedById;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewNotes = req.ReviewNotes;

        tenantUow.Repository<AccidentReport>().Update(report);
        await tenantUow.SaveChangesAsync(ct);

        return Result<AccidentReportDto>.Ok(report.ToDto());
    }
}
