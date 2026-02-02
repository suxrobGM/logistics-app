using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to dismiss a DVIR report (clears without full review, for DVIRs with no defects)
/// </summary>
public record DismissDvirReportCommand : IAppRequest<Result<DvirReportDto>>
{
    public required Guid ReportId { get; set; }
    public required Guid DismissedById { get; set; }
    public string? Notes { get; set; }
}
