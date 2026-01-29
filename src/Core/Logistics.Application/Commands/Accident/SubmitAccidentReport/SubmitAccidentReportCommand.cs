using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record SubmitAccidentReportCommand : IAppRequest<Result<AccidentReportDto>>
{
    public required Guid ReportId { get; set; }
}
