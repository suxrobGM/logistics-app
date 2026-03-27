using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to reject a DVIR report (sends back to driver for resubmission)
/// </summary>
[RequiresFeature(TenantFeature.Safety)]
public record RejectDvirReportCommand : IAppRequest<Result<DvirReportDto>>
{
    public required Guid ReportId { get; set; }
    public required Guid RejectedById { get; set; }
    public required string RejectionReason { get; set; }
}
