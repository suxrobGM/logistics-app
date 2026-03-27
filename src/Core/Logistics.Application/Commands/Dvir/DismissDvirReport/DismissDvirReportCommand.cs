using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to dismiss a DVIR report (clears without full review, for DVIRs with no defects)
/// </summary>
[RequiresFeature(TenantFeature.Safety)]
public record DismissDvirReportCommand : IAppRequest<Result<DvirReportDto>>
{
    public required Guid ReportId { get; set; }
    public required Guid DismissedById { get; set; }
    public string? Notes { get; set; }
}
