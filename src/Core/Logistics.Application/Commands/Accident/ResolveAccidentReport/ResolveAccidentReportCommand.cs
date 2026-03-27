using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to resolve an accident report (changes status to Resolved)
/// </summary>
[RequiresFeature(TenantFeature.Safety)]
public record ResolveAccidentReportCommand : IAppRequest<Result<AccidentReportDto>>
{
    public required Guid ReportId { get; init; }
    public string? ResolutionNotes { get; init; }
}
