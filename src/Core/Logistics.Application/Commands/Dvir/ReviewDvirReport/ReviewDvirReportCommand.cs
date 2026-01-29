using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record ReviewDvirReportCommand : IAppRequest<Result<DvirReportDto>>
{
    public required Guid ReportId { get; set; }
    public required Guid ReviewedById { get; set; }
    public required bool DefectsCorrected { get; set; }
    public string? MechanicSignature { get; set; }
    public string? MechanicNotes { get; set; }
}
