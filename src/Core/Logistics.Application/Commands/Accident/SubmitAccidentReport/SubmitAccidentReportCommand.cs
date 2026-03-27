using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Safety)]
public record SubmitAccidentReportCommand : IAppRequest<Result<AccidentReportDto>>
{
    public required Guid ReportId { get; set; }
}
