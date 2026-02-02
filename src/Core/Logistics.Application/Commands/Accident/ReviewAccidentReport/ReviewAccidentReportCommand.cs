using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to review a submitted accident report (changes status to UnderReview)
/// </summary>
public record ReviewAccidentReportCommand : IAppRequest<Result<AccidentReportDto>>
{
    public required Guid ReportId { get; init; }
    public required Guid ReviewedById { get; init; }
    public string? ReviewNotes { get; init; }
}
