using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Commands;

[RequiresFeature(TenantFeature.Safety)]
public record SubmitAccidentReportCommand : ICommand<Result<AccidentReportDto>>
{
    public required Guid ReportId { get; set; }
}
