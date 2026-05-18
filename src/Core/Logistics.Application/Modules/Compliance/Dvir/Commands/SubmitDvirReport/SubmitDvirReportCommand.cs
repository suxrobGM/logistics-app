using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Commands;

[RequiresFeature(TenantFeature.Safety)]
public record SubmitDvirReportCommand : ICommand<Result<DvirReportDto>>
{
    public required Guid ReportId { get; set; }
    public string? DriverSignature { get; set; }
}
