using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Commands;

/// <summary>
/// Command to review a submitted accident report (changes status to UnderReview)
/// </summary>
[RequiresFeature(TenantFeature.Safety)]
public record ReviewAccidentReportCommand : ICommand<Result<AccidentReportDto>>
{
    public required Guid ReportId { get; init; }
    public string? ReviewNotes { get; init; }
}
