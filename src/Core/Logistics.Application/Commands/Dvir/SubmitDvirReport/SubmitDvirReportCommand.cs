using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record SubmitDvirReportCommand : IAppRequest<Result<DvirReportDto>>
{
    public required Guid ReportId { get; set; }
    public string? DriverSignature { get; set; }
}
