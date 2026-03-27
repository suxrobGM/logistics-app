using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Safety)]
public record ReviewDvirReportCommand : IAppRequest<Result<DvirReportDto>>
{
    public required Guid ReportId { get; set; }
    public required Guid ReviewedById { get; set; }
    public required bool DefectsCorrected { get; set; }
    public string? MechanicSignature { get; set; }
    public string? MechanicNotes { get; set; }
}
