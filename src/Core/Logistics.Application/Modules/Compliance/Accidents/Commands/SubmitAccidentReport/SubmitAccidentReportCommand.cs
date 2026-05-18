using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Accidents.Commands;

[RequiresFeature(TenantFeature.Safety)]
public record SubmitAccidentReportCommand : ICommand<Result<AccidentReportDto>>
{
    public required Guid ReportId { get; set; }
}
