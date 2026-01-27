using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class ReportLoadExceptionCommand : IAppRequest<Result>
{
    public Guid LoadId { get; set; }
    public LoadExceptionType Type { get; set; }
    public required string Reason { get; set; }
}
