using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Commands;

internal sealed class ResolveAccidentReportHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<ResolveAccidentReportCommand, Result<AccidentReportDto>>
{
    public async Task<Result<AccidentReportDto>> Handle(ResolveAccidentReportCommand req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<AccidentReport>().GetByIdAsync(req.ReportId, ct);
        if (report is null)
        {
            return Result<AccidentReportDto>.Fail("Accident report not found.", ErrorCodes.NotFound);
        }

        // Allow resolving from submitted or under_review status
        if (report.Status != AccidentReportStatus.Submitted &&
            report.Status != AccidentReportStatus.UnderReview &&
            report.Status != AccidentReportStatus.InsuranceFiled)
        {
            return Result<AccidentReportDto>.Fail("Only submitted, under review, or insurance filed reports can be resolved.");
        }

        report.Status = AccidentReportStatus.Resolved;

        // Append resolution notes to review notes if exists
        if (!string.IsNullOrEmpty(req.ResolutionNotes))
        {
            report.ReviewNotes = string.IsNullOrEmpty(report.ReviewNotes)
                ? $"Resolution: {req.ResolutionNotes}"
                : $"{report.ReviewNotes}\n\nResolution: {req.ResolutionNotes}";
        }

        await tenantUow.SaveChangesAsync(ct);

        return Result<AccidentReportDto>.Ok(report.ToDto());
    }
}
