using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to review a submitted accident report (changes status to UnderReview)
/// </summary>
[RequiresFeature(TenantFeature.Safety)]
public record ReviewAccidentReportCommand : IAppRequest<Result<AccidentReportDto>>
{
    public required Guid ReportId { get; init; }
    public required Guid ReviewedById { get; init; }
    public string? ReviewNotes { get; init; }
}
